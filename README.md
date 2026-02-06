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
dotnet build
```
If your environment cancels parallel builds, use:
```bash
dotnet build -m:1
```

## Test
```bash
dotnet test
```
If your environment cancels parallel builds, use:
```bash
dotnet test -m:1
```

## Run WebApi
```bash
dotnet run --project src/Pos.WebApi/Pos.WebApi.csproj
```

Sample endpoint:
- `GET /api/v1/health`
