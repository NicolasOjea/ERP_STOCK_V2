namespace Pos.Application.DTOs.Products;

public sealed record ProductListItemDto(
    Guid Id,
    string Name,
    string Sku,
    Guid? CategoriaId,
    string? Categoria,
    Guid? MarcaId,
    string? Marca,
    Guid? ProveedorId,
    string? Proveedor,
    bool IsActive);
