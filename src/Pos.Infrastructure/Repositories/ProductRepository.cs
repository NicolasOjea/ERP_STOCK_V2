using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Pos.Application.Abstractions;
using Pos.Application.DTOs.Products;
using Pos.Application.DTOs.Etiquetas;
using Pos.Domain.Entities;
using Pos.Domain.Exceptions;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly PosDbContext _dbContext;

    public ProductRepository(PosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductListItemDto>> SearchAsync(
        Guid tenantId,
        string? search,
        Guid? categoriaId,
        bool? activo,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Productos.AsNoTracking()
            .Where(p => p.TenantId == tenantId);

        if (categoriaId.HasValue)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        if (activo.HasValue)
        {
            query = query.Where(p => p.IsActive == activo.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{term}%")
                || EF.Functions.ILike(p.Sku, $"%{term}%")
                || _dbContext.ProductoCodigos.AsNoTracking().Any(c =>
                    c.TenantId == tenantId
                    && c.ProductoId == p.Id
                    && EF.Functions.ILike(c.Codigo, $"%{term}%")));
        }

        var results = await (from p in query
                join c in _dbContext.Categorias.AsNoTracking().Where(c => c.TenantId == tenantId)
                    on p.CategoriaId equals c.Id into cat
                from c in cat.DefaultIfEmpty()
                join m in _dbContext.Marcas.AsNoTracking().Where(m => m.TenantId == tenantId)
                    on p.MarcaId equals m.Id into mar
                from m in mar.DefaultIfEmpty()
                join pr in _dbContext.Proveedores.AsNoTracking().Where(pr => pr.TenantId == tenantId)
                    on p.ProveedorId equals pr.Id into prov
                from pr in prov.DefaultIfEmpty()
                orderby p.Name
                select new ProductListItemDto(
                    p.Id,
                    p.Name,
                    p.Sku,
                    p.CategoriaId,
                    c != null ? c.Name : null,
                    p.MarcaId,
                    m != null ? m.Name : null,
                    p.ProveedorId,
                    pr != null ? pr.Name : null,
                    p.IsActive))
            .ToListAsync(cancellationToken);

        return results;
    }

    public async Task<ProductDetailDto?> GetByIdAsync(Guid tenantId, Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await (from p in _dbContext.Productos.AsNoTracking()
                where p.TenantId == tenantId && p.Id == productId
                join c in _dbContext.Categorias.AsNoTracking().Where(c => c.TenantId == tenantId)
                    on p.CategoriaId equals c.Id into cat
                from c in cat.DefaultIfEmpty()
                join m in _dbContext.Marcas.AsNoTracking().Where(m => m.TenantId == tenantId)
                    on p.MarcaId equals m.Id into mar
                from m in mar.DefaultIfEmpty()
                join pr in _dbContext.Proveedores.AsNoTracking().Where(pr => pr.TenantId == tenantId)
                    on p.ProveedorId equals pr.Id into prov
                from pr in prov.DefaultIfEmpty()
                select new
                {
                    Product = p,
                    Categoria = c != null ? c.Name : null,
                    Marca = m != null ? m.Name : null,
                    Proveedor = pr != null ? pr.Name : null
                })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return null;
        }

        var codes = await _dbContext.ProductoCodigos.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.ProductoId == productId)
            .OrderBy(c => c.Codigo)
            .Select(c => new ProductCodeDto(c.Id, c.Codigo))
            .ToListAsync(cancellationToken);

        return new ProductDetailDto(
            product.Product.Id,
            product.Product.Name,
            product.Product.Sku,
            product.Product.CategoriaId,
            product.Categoria,
            product.Product.MarcaId,
            product.Marca,
            product.Product.ProveedorId,
            product.Proveedor,
            product.Product.IsActive,
            codes);
    }

    public async Task<Guid> CreateAsync(
        Guid tenantId,
        ProductCreateDto request,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var skuExists = await _dbContext.Productos.AsNoTracking()
            .AnyAsync(p => p.TenantId == tenantId && p.Sku == request.Sku, cancellationToken);

        if (skuExists)
        {
            throw new ConflictException("SKU ya existe.");
        }

        if (request.CategoriaId.HasValue)
        {
            var categoriaExists = await _dbContext.Categorias.AsNoTracking()
                .AnyAsync(c => c.TenantId == tenantId && c.Id == request.CategoriaId.Value, cancellationToken);

            if (!categoriaExists)
            {
                throw new ValidationException(
                    "Validacion fallida.",
                    new Dictionary<string, string[]>
                    {
                        ["categoriaId"] = new[] { "La categoria no existe." }
                    });
            }
        }

        if (request.MarcaId.HasValue)
        {
            var marcaExists = await _dbContext.Marcas.AsNoTracking()
                .AnyAsync(m => m.TenantId == tenantId && m.Id == request.MarcaId.Value, cancellationToken);

            if (!marcaExists)
            {
                throw new ValidationException(
                    "Validacion fallida.",
                    new Dictionary<string, string[]>
                    {
                        ["marcaId"] = new[] { "La marca no existe." }
                    });
            }
        }

        if (request.ProveedorId.HasValue)
        {
            var proveedorExists = await _dbContext.Proveedores.AsNoTracking()
                .AnyAsync(p => p.TenantId == tenantId && p.Id == request.ProveedorId.Value, cancellationToken);

            if (!proveedorExists)
            {
                throw new ValidationException(
                    "Validacion fallida.",
                    new Dictionary<string, string[]>
                    {
                        ["proveedorId"] = new[] { "El proveedor no existe." }
                    });
            }
        }

        var product = new Producto(
            Guid.NewGuid(),
            tenantId,
            request.Name,
            request.Sku,
            request.CategoriaId,
            request.MarcaId,
            request.ProveedorId,
            nowUtc,
            request.PrecioBase ?? 1m,
            request.IsActive ?? true);

        _dbContext.Productos.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (request.ProveedorId.HasValue)
        {
            var relation = new ProductoProveedor(
                Guid.NewGuid(),
                tenantId,
                product.Id,
                request.ProveedorId.Value,
                true,
                nowUtc);
            _dbContext.ProductoProveedores.Add(relation);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return product.Id;
    }

    public async Task<bool> UpdateAsync(
        Guid tenantId,
        Guid productId,
        ProductUpdateDto request,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Productos
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == productId, cancellationToken);

        if (product is null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Sku) && !string.Equals(request.Sku, product.Sku, StringComparison.OrdinalIgnoreCase))
        {
            var skuExists = await _dbContext.Productos.AsNoTracking()
                .AnyAsync(p => p.TenantId == tenantId && p.Sku == request.Sku, cancellationToken);

            if (skuExists)
            {
                throw new ConflictException("SKU ya existe.");
            }
        }

        if (request.CategoriaId.HasValue)
        {
            var categoriaExists = await _dbContext.Categorias.AsNoTracking()
                .AnyAsync(c => c.TenantId == tenantId && c.Id == request.CategoriaId.Value, cancellationToken);

            if (!categoriaExists)
            {
                throw new ValidationException(
                    "Validacion fallida.",
                    new Dictionary<string, string[]>
                    {
                        ["categoriaId"] = new[] { "La categoria no existe." }
                    });
            }
        }

        if (request.MarcaId.HasValue)
        {
            var marcaExists = await _dbContext.Marcas.AsNoTracking()
                .AnyAsync(m => m.TenantId == tenantId && m.Id == request.MarcaId.Value, cancellationToken);

            if (!marcaExists)
            {
                throw new ValidationException(
                    "Validacion fallida.",
                    new Dictionary<string, string[]>
                    {
                        ["marcaId"] = new[] { "La marca no existe." }
                    });
            }
        }

        if (request.ProveedorId.HasValue)
        {
            var proveedorExists = await _dbContext.Proveedores.AsNoTracking()
                .AnyAsync(p => p.TenantId == tenantId && p.Id == request.ProveedorId.Value, cancellationToken);

            if (!proveedorExists)
            {
                throw new ValidationException(
                    "Validacion fallida.",
                    new Dictionary<string, string[]>
                    {
                        ["proveedorId"] = new[] { "El proveedor no existe." }
                    });
            }
        }

        var newName = string.IsNullOrWhiteSpace(request.Name) ? product.Name : request.Name;
        var newSku = string.IsNullOrWhiteSpace(request.Sku) ? product.Sku : request.Sku;
        var newCategoriaId = request.CategoriaId ?? product.CategoriaId;
        var newMarcaId = request.MarcaId ?? product.MarcaId;
        var newProveedorId = request.ProveedorId ?? product.ProveedorId;
        var newIsActive = request.IsActive ?? product.IsActive;
        var newPrecioBase = request.PrecioBase ?? product.PrecioBase;

        product.Update(newName, newSku, newCategoriaId, newMarcaId, newProveedorId, newPrecioBase, newIsActive, nowUtc);

        if (request.ProveedorId.HasValue)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var relations = await _dbContext.ProductoProveedores
                .Where(r => r.TenantId == tenantId && r.ProductoId == productId)
                .ToListAsync(cancellationToken);

            var relation = relations.FirstOrDefault(r => r.ProveedorId == request.ProveedorId.Value);
            if (relation is null)
            {
                relation = new ProductoProveedor(
                    Guid.NewGuid(),
                    tenantId,
                    productId,
                    request.ProveedorId.Value,
                    true,
                    nowUtc);
                _dbContext.ProductoProveedores.Add(relation);
            }

            foreach (var rel in relations.Where(r => r.EsPrincipal && r.Id != relation.Id))
            {
                rel.SetPrincipal(false, nowUtc);
            }

            if (!relation.EsPrincipal)
            {
                relation.SetPrincipal(true, nowUtc);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProductCodeDto?> AddCodeAsync(
        Guid tenantId,
        Guid productId,
        string code,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var productExists = await _dbContext.Productos.AsNoTracking()
            .AnyAsync(p => p.TenantId == tenantId && p.Id == productId, cancellationToken);

        if (!productExists)
        {
            return null;
        }

        var codeExists = await _dbContext.ProductoCodigos.AsNoTracking()
            .AnyAsync(c => c.TenantId == tenantId && c.Codigo == code, cancellationToken);

        if (codeExists)
        {
            throw new ConflictException("Codigo ya existe");
        }

        var entity = new ProductoCodigo(Guid.NewGuid(), tenantId, productId, code, nowUtc);
        _dbContext.ProductoCodigos.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProductCodeDto(entity.Id, entity.Codigo);
    }

    public async Task<ProductCodeDto?> RemoveCodeAsync(
        Guid tenantId,
        Guid productId,
        Guid codeId,
        CancellationToken cancellationToken = default)
    {
        var code = await _dbContext.ProductoCodigos
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.ProductoId == productId && c.Id == codeId, cancellationToken);

        if (code is null)
        {
            return null;
        }

        var dto = new ProductCodeDto(code.Id, code.Codigo);
        _dbContext.ProductoCodigos.Remove(code);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return dto;
    }

    public async Task<ProductProveedorDto?> AddProveedorAsync(
        Guid tenantId,
        Guid productId,
        Guid proveedorId,
        bool esPrincipal,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var product = await _dbContext.Productos
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == productId, cancellationToken);

        if (product is null)
        {
            return null;
        }

        var proveedorExists = await _dbContext.Proveedores.AsNoTracking()
            .AnyAsync(p => p.TenantId == tenantId && p.Id == proveedorId, cancellationToken);

        if (!proveedorExists)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["proveedorId"] = new[] { "El proveedor no existe." }
                });
        }

        var relations = await _dbContext.ProductoProveedores
            .Where(r => r.TenantId == tenantId && r.ProductoId == productId)
            .ToListAsync(cancellationToken);

        var relation = relations.FirstOrDefault(r => r.ProveedorId == proveedorId);
        if (relation is null)
        {
            relation = new ProductoProveedor(Guid.NewGuid(), tenantId, productId, proveedorId, esPrincipal, nowUtc);
            _dbContext.ProductoProveedores.Add(relation);
        }

        if (esPrincipal)
        {
            foreach (var rel in relations.Where(r => r.EsPrincipal && r.Id != relation.Id))
            {
                rel.SetPrincipal(false, nowUtc);
            }

            if (!relation.EsPrincipal)
            {
                relation.SetPrincipal(true, nowUtc);
            }

            product.SetProveedorPrincipal(proveedorId, nowUtc);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var proveedor = await _dbContext.Proveedores.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.Id == proveedorId)
            .Select(p => new { p.Id, p.Name })
            .FirstOrDefaultAsync(cancellationToken);

        var nombre = proveedor?.Name ?? string.Empty;
        return new ProductProveedorDto(relation.Id, proveedorId, nombre, relation.EsPrincipal);
    }

    public async Task<ProductProveedorDto?> SetProveedorPrincipalAsync(
        Guid tenantId,
        Guid productId,
        Guid relationId,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var product = await _dbContext.Productos
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == productId, cancellationToken);

        if (product is null)
        {
            return null;
        }

        var relations = await _dbContext.ProductoProveedores
            .Where(r => r.TenantId == tenantId && r.ProductoId == productId)
            .ToListAsync(cancellationToken);

        var relation = relations.FirstOrDefault(r => r.Id == relationId);
        if (relation is null)
        {
            return null;
        }

        foreach (var rel in relations.Where(r => r.EsPrincipal && r.Id != relation.Id))
        {
            rel.SetPrincipal(false, nowUtc);
        }

        if (!relation.EsPrincipal)
        {
            relation.SetPrincipal(true, nowUtc);
        }

        product.SetProveedorPrincipal(relation.ProveedorId, nowUtc);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var proveedor = await _dbContext.Proveedores.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.Id == relation.ProveedorId)
            .Select(p => new { p.Id, p.Name })
            .FirstOrDefaultAsync(cancellationToken);

        var nombre = proveedor?.Name ?? string.Empty;
        return new ProductProveedorDto(relation.Id, relation.ProveedorId, nombre, relation.EsPrincipal);
    }

    public async Task<Guid?> GetIdBySkuAsync(
        Guid tenantId,
        string sku,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return null;
        }

        var normalized = sku.Trim();
        return await _dbContext.Productos.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.Sku == normalized)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid?> GetIdByCodeAsync(
        Guid tenantId,
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var normalized = code.Trim();
        return await _dbContext.ProductoCodigos.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.Codigo == normalized)
            .Select(c => (Guid?)c.ProductoId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EtiquetaItemDto>> GetLabelDataAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> productIds,
        string listaPrecio,
        CancellationToken cancellationToken = default)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return Array.Empty<EtiquetaItemDto>();
        }

        var products = await _dbContext.Productos.AsNoTracking()
            .Where(p => p.TenantId == tenantId && ids.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Sku,
                p.PrecioBase
            })
            .ToListAsync(cancellationToken);

        var codes = await _dbContext.ProductoCodigos.AsNoTracking()
            .Where(c => c.TenantId == tenantId && ids.Contains(c.ProductoId))
            .OrderBy(c => c.Codigo)
            .Select(c => new { c.ProductoId, c.Codigo })
            .ToListAsync(cancellationToken);

        var codeByProduct = codes
            .GroupBy(c => c.ProductoId)
            .ToDictionary(g => g.Key, g => g.First().Codigo);

        Dictionary<Guid, decimal> prices = new();
        if (!string.IsNullOrWhiteSpace(listaPrecio))
        {
            var lista = await _dbContext.ListasPrecio.AsNoTracking()
                .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Nombre == listaPrecio, cancellationToken);

            if (lista is not null)
            {
                prices = await _dbContext.ListaPrecioItems.AsNoTracking()
                    .Where(i => i.TenantId == tenantId && i.ListaPrecioId == lista.Id && ids.Contains(i.ProductoId))
                    .ToDictionaryAsync(i => i.ProductoId, i => i.Precio, cancellationToken);
            }
        }

        var result = products
            .OrderBy(p => p.Name)
            .Select(p =>
            {
                var codigo = codeByProduct.TryGetValue(p.Id, out var c) ? c : p.Sku;
                var precio = prices.TryGetValue(p.Id, out var price) ? price : p.PrecioBase;
                return new EtiquetaItemDto(p.Id, p.Name, precio, codigo);
            })
            .ToList();

        return result;
    }
}

