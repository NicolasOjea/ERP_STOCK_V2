using Pos.Domain.Common;

namespace Pos.Domain.Entities;

public sealed class Producto : EntityBase
{
    private Producto()
    {
    }

    public Producto(
        Guid id,
        Guid tenantId,
        string name,
        string sku,
        Guid? categoriaId,
        Guid? marcaId,
        Guid? proveedorId,
        DateTimeOffset createdAtUtc,
        decimal precioBase = 1m,
        bool isActive = true)
        : base(id, tenantId, createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("Sku is required.", nameof(sku));
        if (precioBase < 0) throw new ArgumentException("PrecioBase must be >= 0.", nameof(precioBase));

        Name = name;
        Sku = sku;
        CategoriaId = categoriaId;
        MarcaId = marcaId;
        ProveedorId = proveedorId;
        PrecioBase = precioBase;
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public Guid? CategoriaId { get; private set; }
    public Guid? MarcaId { get; private set; }
    public Guid? ProveedorId { get; private set; }
    public decimal PrecioBase { get; private set; }
    public bool IsActive { get; private set; }

    public void Update(
        string name,
        string sku,
        Guid? categoriaId,
        Guid? marcaId,
        Guid? proveedorId,
        decimal precioBase,
        bool isActive,
        DateTimeOffset updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("Sku is required.", nameof(sku));
        if (precioBase < 0) throw new ArgumentException("PrecioBase must be >= 0.", nameof(precioBase));

        Name = name;
        Sku = sku;
        CategoriaId = categoriaId;
        MarcaId = marcaId;
        ProveedorId = proveedorId;
        PrecioBase = precioBase;
        IsActive = isActive;
        MarkUpdated(updatedAtUtc);
    }

    public void SetProveedorPrincipal(Guid? proveedorId, DateTimeOffset updatedAtUtc)
    {
        ProveedorId = proveedorId;
        MarkUpdated(updatedAtUtc);
    }
}
