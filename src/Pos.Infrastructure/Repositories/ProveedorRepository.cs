using Microsoft.EntityFrameworkCore;
using Pos.Application.Abstractions;
using Pos.Application.DTOs.Proveedores;
using Pos.Domain.Entities;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Repositories;

public sealed class ProveedorRepository : IProveedorRepository
{
    private readonly PosDbContext _dbContext;

    public ProveedorRepository(PosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProveedorDto>> SearchAsync(
        Guid tenantId,
        string? search,
        bool? activo,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Proveedores.AsNoTracking()
            .Where(p => p.TenantId == tenantId);

        if (activo.HasValue)
        {
            query = query.Where(p => p.IsActive == activo.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{term}%")
                || EF.Functions.ILike(p.Telefono, $"%{term}%")
                || (p.Cuit != null && EF.Functions.ILike(p.Cuit, $"%{term}%")));
        }

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new ProveedorDto(p.Id, p.Name, p.Telefono, p.Cuit, p.Direccion, p.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(
        Guid tenantId,
        ProveedorCreateDto request,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var proveedor = new Proveedor(
            Guid.NewGuid(),
            tenantId,
            request.Name,
            request.Telefono,
            request.Cuit,
            request.Direccion,
            nowUtc,
            request.IsActive ?? true);
        _dbContext.Proveedores.Add(proveedor);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return proveedor.Id;
    }

    public async Task<bool> UpdateAsync(
        Guid tenantId,
        Guid proveedorId,
        ProveedorUpdateDto request,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var proveedor = await _dbContext.Proveedores
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == proveedorId, cancellationToken);

        if (proveedor is null)
        {
            return false;
        }

        var newName = request.Name is null ? proveedor.Name : request.Name;
        var newTelefono = request.Telefono is null ? proveedor.Telefono : request.Telefono;
        var newCuit = request.Cuit is null ? proveedor.Cuit : request.Cuit;
        var newDireccion = request.Direccion is null ? proveedor.Direccion : request.Direccion;
        var newIsActive = request.IsActive ?? proveedor.IsActive;

        proveedor.Update(newName, newTelefono, newCuit, newDireccion, newIsActive, nowUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProveedorDto?> GetByIdAsync(
        Guid tenantId,
        Guid proveedorId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Proveedores.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.Id == proveedorId)
            .Select(p => new ProveedorDto(p.Id, p.Name, p.Telefono, p.Cuit, p.Direccion, p.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
