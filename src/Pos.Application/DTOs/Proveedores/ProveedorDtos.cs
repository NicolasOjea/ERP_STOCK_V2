namespace Pos.Application.DTOs.Proveedores;

public sealed record ProveedorDto(
    Guid Id,
    string Name,
    string Telefono,
    string? Cuit,
    string? Direccion,
    bool IsActive);

public sealed record ProveedorCreateDto(
    string Name,
    string Telefono,
    string? Cuit,
    string? Direccion,
    bool? IsActive);

public sealed record ProveedorUpdateDto(
    string? Name,
    string? Telefono,
    string? Cuit,
    string? Direccion,
    bool? IsActive);
