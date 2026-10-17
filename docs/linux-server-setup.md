# Savor Linux server setup

This guide describes a simple Linux deployment for the Savor API:

```text
Internet
    -> reverse proxy with HTTPS (optional but recommended)
        -> Savor API on localhost:5093
            -> PostgreSQL in Docker on localhost:55432
```

The commands below target Ubuntu 24.04 or a similar Debian-based distribution.
Adjust package-management and firewall commands for another distribution.

## 1. Prerequisites

Install:

- .NET SDK 10 for migrations and building.
- .NET ASP.NET Core Runtime 10 if the server will only run a published
  framework-dependent application.
- Docker Engine and the Docker Compose plugin.
- Git.
- A reverse proxy such as Nginx or Caddy if the API will be exposed publicly.

Verify the installation:

```bash
dotnet --info
docker --version
docker compose version
git --version
```

Create a dedicated service account and application directory:

```bash
sudo useradd --system --create-home --shell /usr/sbin/nologin savor
sudo mkdir -p /opt/savor
sudo chown -R "$USER":"$USER" /opt/savor
```

The account that runs Docker must have permission to access the Docker socket.
Use a deployment account with Docker access, or add the account to the Docker
group and start a new login session:

```bash
sudo usermod -aG docker "$USER"
```

Docker group membership grants root-equivalent privileges. Use a restricted
deployment process if that is not acceptable for your server.

## 2. Get the application

Clone the repository into the application directory:

```bash
cd /opt
sudo git clone <repository-url> savor
sudo chown -R "$USER":"$USER" /opt/savor
cd /opt/savor
```

For a deployment, check out a known release commit or tag rather than running
an unreviewed branch:

```bash
git checkout <release-or-commit>
```

## 3. Configure PostgreSQL

The repository's Compose file maps PostgreSQL to host port `55432`. This avoids
the common conflict with a PostgreSQL service already using host port `5432`.
The container itself continues to listen on its normal port `5432`.

Review the credentials in
[docker-compose.yml](../deploy/docker/docker-compose.yml) before production
use. The example values are development credentials and should be changed for
a real server.

Start PostgreSQL:

```bash
cd /opt/savor
docker compose -f deploy/docker/docker-compose.yml up -d
docker compose -f deploy/docker/docker-compose.yml ps
```

Wait until the container reports `healthy`:

```bash
until docker compose -f deploy/docker/docker-compose.yml ps \
  --format json | grep -q healthy; do
  sleep 2
done
```

Do not expose PostgreSQL to the public internet. If the database is only used
by this server, bind the host port to loopback in the Compose file:

```yaml
ports:
  - "127.0.0.1:55432:5432"
```

## 4. Configure the API

Do not put production credentials in tracked JSON files. Create a root-owned
environment file:

```bash
sudo install -d -m 0750 /etc/savor
sudo install -m 0600 /dev/null /etc/savor/savor.env
sudo nano /etc/savor/savor.env
```

Add:

```dotenv
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Database=Host=127.0.0.1;Port=55432;Database=savor;Username=savor;Password=<strong-password>
ASPNETCORE_URLS=http://127.0.0.1:5093
```

The database password must match the PostgreSQL password configured for the
container. Use a strong unique password in production.

## 5. Apply migrations

For a server that has the .NET SDK installed, load the environment file and
apply the migration:

```bash
set -a
. /etc/savor/savor.env
set +a

dotnet tool install --global dotnet-ef --version 10.0.13
export PATH="$PATH:$HOME/.dotnet/tools"

dotnet ef database update \
  --project src/Savor.Infrastructure \
  --startup-project src/Savor.Api
```

If `dotnet-ef` is already installed, update it only when the project version
changes:

```bash
dotnet tool update --global dotnet-ef --version 10.0.13
```

For larger production deployments, run migrations as an explicit release step
before switching the application to the new version. Do not let multiple API
instances attempt migrations concurrently.

## 6. Publish the API

The simplest deployment is framework-dependent:

```bash
cd /opt/savor
dotnet publish src/Savor.Api/Savor.Api.csproj \
  --configuration Release \
  --output /opt/savor/current
```

The server must have the matching ASP.NET Core 10 runtime installed. Check it
with:

```bash
dotnet --list-runtimes
```

If the server should not install the .NET runtime separately, publish
self-contained for the server architecture. For a typical 64-bit Linux host:

```bash
dotnet publish src/Savor.Api/Savor.Api.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  --output /opt/savor/current
```

The service command in the next section differs slightly for a
self-contained deployment.

## 7. Run Savor with systemd

Create the service definition:

```bash
sudo nano /etc/systemd/system/savor-api.service
```

For a framework-dependent deployment, use:

```ini
[Unit]
Description=Savor API
After=docker.service network-online.target
Requires=docker.service
Wants=network-online.target

[Service]
Type=notify
User=savor
Group=savor
WorkingDirectory=/opt/savor/current
EnvironmentFile=/etc/savor/savor.env
ExecStart=/usr/bin/dotnet /opt/savor/current/Savor.Api.dll
Restart=on-failure
RestartSec=5
TimeoutStopSec=30
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/opt/savor

[Install]
WantedBy=multi-user.target
```

For a self-contained deployment, replace `ExecStart` with:

```ini
ExecStart=/opt/savor/current/Savor.Api
```

Enable and start the service:

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now savor-api
sudo systemctl status savor-api
```

View logs:

```bash
sudo journalctl -u savor-api -f
```

Verify the API locally on the server:

```bash
curl --fail http://127.0.0.1:5093/health
curl --fail http://127.0.0.1:5093/recipes/
```

## 8. Expose the API through a reverse proxy

Do not expose Kestrel directly to the public internet without HTTPS. Use a
reverse proxy and keep Kestrel bound to `127.0.0.1`.

For Nginx:

```bash
sudo apt update
sudo apt install nginx
```

Create `/etc/nginx/sites-available/savor`:

```nginx
server {
    listen 80;
    listen [::]:80;
    server_name api.example.com;

    location / {
        proxy_pass http://127.0.0.1:5093;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable it:

```bash
sudo ln -s /etc/nginx/sites-available/savor /etc/nginx/sites-enabled/savor
sudo nginx -t
sudo systemctl reload nginx
```

Then install a certificate with Certbot or use another certificate-management
solution:

```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d api.example.com
```

After HTTPS is enabled, verify:

```bash
curl --fail https://api.example.com/health
curl --fail https://api.example.com/recipes/
```

OpenAPI is available only when the ASP.NET Core environment is Development.
Do not enable Development in a public production deployment.

## 9. Firewall

Only expose the ports required by the deployment. With Nginx in front of the
API, that normally means SSH, HTTP, and HTTPS:

```bash
sudo ufw allow OpenSSH
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
sudo ufw status
```

Do not open port `55432` publicly.

## 10. Updating the application

A simple update procedure is:

```bash
cd /opt/savor
git fetch --tags
git checkout <new-release-or-commit>

set -a
. /etc/savor/savor.env
set +a

dotnet ef database update \
  --project src/Savor.Infrastructure \
  --startup-project src/Savor.Api

rm -rf /opt/savor/next
dotnet publish src/Savor.Api/Savor.Api.csproj \
  --configuration Release \
  --output /opt/savor/next

sudo systemctl stop savor-api
rm -rf /opt/savor/current
mv /opt/savor/next /opt/savor/current
sudo systemctl start savor-api
sudo systemctl status savor-api
```

For zero-downtime deployments, use versioned release directories and an
atomic symlink switch instead of removing the current directory.

## 11. Backups

Back up PostgreSQL regularly. A basic logical backup is:

```bash
mkdir -p /opt/savor/backups
docker exec savor-postgres pg_dump \
  -U savor \
  -d savor \
  > /opt/savor/backups/savor-$(date +%Y%m%d-%H%M%S).sql
```

Keep backups outside the server as well. Test restoring a backup before
depending on it:

```bash
docker exec -i savor-postgres psql -U savor -d savor \
  < /opt/savor/backups/<backup-file>.sql
```

The restore command above is suitable for a disposable database. For a real
restore, use a separate database or server and follow the PostgreSQL restore
procedure appropriate for your backup format.

## 12. Troubleshooting

### Password authentication failed

Check that:

```bash
grep '^ConnectionStrings__Database=' /etc/savor/savor.env
docker compose -f deploy/docker/docker-compose.yml ps
```

The connection string must use port `55432` and the same password configured
for the PostgreSQL container. Confirm which process owns the port:

```bash
sudo ss -ltnp | grep -E ':55432|:5432'
```

### The API cannot connect to PostgreSQL

Check the container:

```bash
docker compose -f deploy/docker/docker-compose.yml logs postgres
docker compose -f deploy/docker/docker-compose.yml ps
```

Check the API:

```bash
sudo journalctl -u savor-api -n 100 --no-pager
```

### The API is running but cannot be reached externally

Verify that:

- `savor-api` is active.
- Kestrel is listening on `127.0.0.1:5093`.
- Nginx configuration passes `nginx -t`.
- DNS points to the server.
- Firewall allows ports 80 and 443.

## Security checklist

- Use a unique production database password.
- Keep `/etc/savor/savor.env` readable only by root.
- Do not expose PostgreSQL publicly.
- Run the API as the dedicated `savor` user.
- Put HTTPS in front of the API.
- Keep `ASPNETCORE_ENVIRONMENT=Production`.
- Do not expose the Development OpenAPI endpoint publicly.
- Back up the database and test restoration.
- Keep the operating system, Docker, .NET runtime, and reverse proxy updated.
