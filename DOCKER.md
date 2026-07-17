# Docker Compose Setup

This folder contains a Docker Compose setup for the .NET 8 API and SQL Server.

## Files

- `docker-compose.yml`: runs the API and SQL Server containers.
- `TravelToursWebsite.Api/Dockerfile`: builds and publishes the API container.
- `.dockerignore`: keeps build output, git files, and local secrets out of Docker build context.
- `.env`: local Docker secrets/config values. This file is ignored by git.
- `.env.example`: safe template for `.env`.

## Where To Edit Secrets

Edit this file:

```text
src/.env
```

Important values:

```text
SQL_SA_PASSWORD=Change_this_SQL_password_123!
JWT_SECRET=Change_this_to_a_long_random_secret_at_least_32_bytes
SEED_ADMIN_USERNAME=admin
SEED_ADMIN_EMAIL=admin@traveltours.com
SEED_ADMIN_PASSWORD=admin123
SEED_ADMIN_UPDATE_PASSWORD=false
SMTP_SERVER=
SMTP_USERNAME=
SMTP_PASSWORD=
CORS_ALLOWED_ORIGIN_0=http://localhost:3000
```

`JWT_SECRET` must be at least 32 bytes. `SQL_SA_PASSWORD` must meet SQL Server password complexity requirements.

Admin seeding is controlled by:

```text
DATABASE_MIGRATE_ON_STARTUP=true
DATABASE_SEED_ON_STARTUP=true
DATABASE_RECREATE_WHEN_SCHEMA_MISSING=true
SEED_ADMIN_USERNAME=admin
SEED_ADMIN_EMAIL=admin@traveltours.com
SEED_ADMIN_PASSWORD=admin123
SEED_ADMIN_UPDATE_PASSWORD=false
```

Set `SEED_ADMIN_UPDATE_PASSWORD=true` only when you want container startup to reset the existing admin password to `SEED_ADMIN_PASSWORD`.

`DATABASE_RECREATE_WHEN_SCHEMA_MISSING=true` is for local Docker development. It lets startup recreate the SQL database when the database exists but the original application tables are missing. Turn it off for shared or production databases.

The committed `.env.example` is only a template. Do not put production secrets in committed files.

## Start Containers

From `src`:

```powershell
docker compose up --build -d
```

If containers are already running and you changed `.env` or code, rerun the same command. Docker Compose will rebuild/recreate the API container and seed the admin during startup.

API URL:

```text
http://localhost:8080/api/v1
```

SQL Server host from your machine:

```text
localhost,1433
```

SQL Server host from inside Docker:

```text
sqlserver,1433
```


## Connect From Windows Database Tools

Use this connection information from SSMS, Azure Data Studio, Rider, LINQPad, or other tools running on Windows:

```text
Server=localhost,1433;Database=TravelToursWebsite;User Id=sa;Password=<SQL_SA_PASSWORD from src/.env>;TrustServerCertificate=True;Encrypt=True;MultipleActiveResultSets=true
```

Use `localhost,1433` or `127.0.0.1,1433` from Windows. The hostname `sqlserver` only works between containers inside Docker Compose, such as from the API container to the SQL Server container.

## Apply Database Migrations

The API does not automatically mutate the database on startup. After SQL Server is healthy, apply EF migrations from your machine:

```powershell
dotnet tool run dotnet-ef database update --project TravelToursWebsite.Infrastructure\TravelToursWebsite.Infrastructure.csproj --startup-project TravelToursWebsite.Api\TravelToursWebsite.Api.csproj
```

If you prefer to use a direct environment variable for the EF design-time factory:

```powershell
$env:TRAVELTOURS_CONNECTION_STRING="Server=localhost,1433;Database=TravelToursWebsite;User Id=sa;Password=<your SQL_SA_PASSWORD>;TrustServerCertificate=True;Encrypt=True;MultipleActiveResultSets=true"
dotnet tool run dotnet-ef database update --project TravelToursWebsite.Infrastructure\TravelToursWebsite.Infrastructure.csproj --startup-project TravelToursWebsite.Api\TravelToursWebsite.Api.csproj
```

## Useful Commands

```powershell
docker compose ps
docker compose logs -f api
docker compose logs -f sqlserver
docker compose down
docker compose down -v
```

Important: `docker compose down -v` deletes the SQL Server data volume, including the `TravelToursWebsite` database. Use it only when you want a fresh local database. The API startup can recreate and seed the local database again when `DATABASE_MIGRATE_ON_STARTUP=true`, `DATABASE_SEED_ON_STARTUP=true`, and `DATABASE_RECREATE_WHEN_SCHEMA_MISSING=true`.

`docker compose down -v` also deletes the uploaded files volume and resource-content volume. Use it only when you are comfortable losing local Docker data.

## Volumes

- `sqlserver-data`: SQL Server database files.
- `api-uploads`: uploaded media under `/app/wwwroot/uploads`.
- `api-resources`: JSON resource files under `/app/Resources`.
