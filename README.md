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

1. Start PostgreSQL (optional for the health endpoint):

```powershell
docker compose -f deploy/docker/docker-compose.yml up -d
```

2. Run the API:

```powershell
dotnet run --project src/Savor.Api
```

3. Open `/health` or the OpenAPI document in Development.

## Configuration

Connection string key: `ConnectionStrings:Database`.

Default local value points at the Docker Compose PostgreSQL service.

## Status

Initial solution bootstrap. Domain schema, auth, web, and mobile clients are not implemented yet.
