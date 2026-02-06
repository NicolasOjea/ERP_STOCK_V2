using Pos.Application.DTOs.Proveedores;

namespace Pos.Application.Abstractions;

public interface IProveedorRepository
{
    Task<IReadOnlyList<ProveedorDto>> SearchAsync(
        Guid tenantId,
        string? search,
        bool? activo,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        Guid tenantId,
        ProveedorCreateDto request,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        Guid tenantId,
        Guid proveedorId,
        ProveedorUpdateDto request,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);

    Task<ProveedorDto?> GetByIdAsync(
        Guid tenantId,
        Guid proveedorId,
        CancellationToken cancellationToken = default);
}
