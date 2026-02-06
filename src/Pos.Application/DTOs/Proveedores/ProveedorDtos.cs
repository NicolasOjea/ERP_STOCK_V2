namespace Pos.Application.DTOs.Proveedores;

public sealed record ProveedorDto(
    Guid Id,
    string Name,
    bool IsActive);

public sealed record ProveedorCreateDto(
    string Name,
    bool? IsActive);

public sealed record ProveedorUpdateDto(
    string? Name,
    bool? IsActive);
