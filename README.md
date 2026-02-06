# POS Solution (Clean/Hexagonal)

## Projects
- Pos.Domain
- Pos.Application
- Pos.Infrastructure
- Pos.WebApi

## Conventions
- IDs: UUID (Guid)
- Base entities: Id, TenantId, CreatedAt, UpdatedAt, DeletedAt (optional)
- API base path: /api/v1

## Build
```bash
dotnet build -m:1
```

## Test
Integration test expects a running Postgres instance. It uses:
- `TEST_DB_CONNECTION` if set
- or defaults to `Host=localhost;Port=5432;Database=posdb;Username=pos;Password=pospass`

```bash
dotnet test -m:1
```

## Migrations
Apply migrations:
```bash
dotnet ef database update -p src/Pos.Infrastructure -s src/Pos.WebApi
```

## Run WebApi (local)
```bash
dotnet run --project src/Pos.WebApi/Pos.WebApi.csproj
```

Sample endpoint:
- `GET /api/v1/health` -> returns status `ok`

## Docker Compose
Start:
```bash
docker compose up -d
```
Logs:
```bash
docker compose logs -f pos-api
```

Optional pgAdmin:
```bash
docker compose --profile tools up -d
```

Health:
```bash
curl http://localhost:8080/api/v1/health
```
