# Savor

Recipe discovery and pantry-matching app for a small group of users. Browse public recipes, keep private ones, filter by tags and ingredients, and request a random recipe.

## Stack

- ASP.NET Core Web API
- PostgreSQL + Entity Framework Core
- Clean architecture: Domain, Application, Infrastructure, Contracts, Api
- Blazor Web and .NET MAUI planned for later vertical slices

## Solution layout

```
src/
  Savor.Api/              # HTTP composition root
  Savor.Application/      # use cases and application policies
  Savor.Domain/           # entities and domain rules
  Savor.Infrastructure/   # EF Core, PostgreSQL, adapters
  Savor.Contracts/        # API DTOs and shared contracts
```

## Prerequisites

- .NET SDK 10
- Docker (optional, for local PostgreSQL)

## Getting started

1. Start PostgreSQL:

```powershell
docker compose -f deploy/docker/docker-compose.yml up -d
```

2. Set the local connection string for the current PowerShell session:

```powershell
$env:ConnectionStrings__Database = "Host=localhost;Port=55432;Database=savor;Username=savor;Password=savor"
```

The password above must match `POSTGRES_PASSWORD` in
[docker-compose.yml](deploy/docker/docker-compose.yml). The application does
not store a database password in tracked JSON configuration.

3. Apply the database migration and seed data:

```powershell
dotnet ef database update `
  --project src/Savor.Infrastructure `
  --startup-project src/Savor.Api
```

The development connection string matches the Docker Compose database. To use a
different database, set `ConnectionStrings__Database` before starting the API.

4. Run the API:

```powershell
dotnet run --project src/Savor.Api
```

5. Open `/health` or the OpenAPI document in Development.

The HTTP launch profile uses `http://localhost:5093`. The HTTPS profile uses
`https://localhost:7249` and `http://localhost:5093`.

## Configuration

Connection string key: `ConnectionStrings:Database`.

The checked-in Development configuration is for local Docker Compose only.
Production deployments should provide the connection string through an
environment variable or another secret configuration provider.

## Testing the current functionality

### Health and OpenAPI

With the API running:

```powershell
Invoke-RestMethod http://localhost:5093/health
Invoke-WebRequest http://localhost:5093/openapi/v1.json
```

The health endpoint should return HTTP 200 and a response with `status` equal
to `Healthy`. The OpenAPI request should return HTTP 200 in Development.

### Public recipe discovery

List all public recipes:

```powershell
Invoke-RestMethod http://localhost:5093/recipes/
```

Filter by tag or ingredient and paginate the result:

```powershell
Invoke-RestMethod 'http://localhost:5093/recipes/?tag=quick'
Invoke-RestMethod 'http://localhost:5093/recipes/?ingredient=tomato&page=1&pageSize=20'
```

Get the seeded public recipe:

```powershell
Invoke-RestMethod `
  http://localhost:5093/recipes/11111111-1111-1111-1111-111111111111
```

The seeded private recipe must not be exposed and should return HTTP 404:

```powershell
try {
    Invoke-WebRequest `
      http://localhost:5093/recipes/22222222-2222-2222-2222-222222222222 `
      -UseBasicParsing
} catch {
    $_.Exception.Response.StatusCode
}
```

The same requests are available in
[Savor.http](src/Savor.Api/Savor.http) and can be run from the VS Code REST
Client extension or another `.http`-file compatible client.

### Build and tests

Build the solution:

```powershell
dotnet build Savor.slnx --configuration Release
```

Run the test suite:

```powershell
dotnet test Savor.slnx --configuration Release
```

The repository includes domain tests and API endpoint tests. The API tests use
a deterministic query-service test double, so they do not require PostgreSQL.

## Status

Public recipe discovery is implemented with PostgreSQL persistence, seeded
development data, filtering, pagination, and OpenAPI documentation.
Authentication, recipe authoring, web, and mobile clients are not implemented
yet.
