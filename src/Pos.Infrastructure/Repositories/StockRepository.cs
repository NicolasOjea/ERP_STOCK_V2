using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Pos.Application.Abstractions;
using Pos.Application.DTOs.Stock;
using Pos.Domain.Entities;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Repositories;

public sealed class StockRepository : IStockRepository
{
    private readonly PosDbContext _dbContext;

    public StockRepository(PosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ProductExistsAsync(Guid tenantId, Guid productId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Productos.AsNoTracking()
            .AnyAsync(p => p.TenantId == tenantId && p.Id == productId, cancellationToken);
    }

    public async Task<StockConfigDto?> GetStockConfigAsync(
        Guid tenantId,
        Guid sucursalId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductoStockConfigs.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.SucursalId == sucursalId && c.ProductoId == productId)
            .Select(c => new StockConfigDto(c.ProductoId, c.SucursalId, c.StockMinimo, c.ToleranciaPct))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<StockConfigDto> UpsertStockConfigAsync(
        Guid tenantId,
        Guid sucursalId,
        Guid productId,
        decimal stockMinimo,
        decimal toleranciaPct,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.ProductoStockConfigs
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.SucursalId == sucursalId && c.ProductoId == productId, cancellationToken);

        if (existing is null)
        {
            existing = new ProductoStockConfig(
                Guid.NewGuid(),
                tenantId,
                productId,
                sucursalId,
                stockMinimo,
                toleranciaPct,
                nowUtc);
            _dbContext.ProductoStockConfigs.Add(existing);
        }
        else
        {
            existing.Update(stockMinimo, toleranciaPct, nowUtc);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new StockConfigDto(existing.ProductoId, existing.SucursalId, existing.StockMinimo, existing.ToleranciaPct);
    }

    public async Task<IReadOnlyList<StockSaldoDto>> GetSaldosAsync(
        Guid tenantId,
        Guid sucursalId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var productsQuery = _dbContext.Productos.AsNoTracking()
            .Where(p => p.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            productsQuery = productsQuery.Where(p =>
                EF.Functions.ILike(p.Name, $"%{term}%")
                || EF.Functions.ILike(p.Sku, $"%{term}%")
                || _dbContext.ProductoCodigos.AsNoTracking().Any(c =>
                    c.TenantId == tenantId
                    && c.ProductoId == p.Id
                    && EF.Functions.ILike(c.Codigo, $"%{term}%")));
        }

        var products = await productsQuery
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.Sku })
            .ToListAsync(cancellationToken);

        if (products.Count == 0)
        {
            return Array.Empty<StockSaldoDto>();
        }

        var productIds = products.Select(p => p.Id).ToList();

        var existingSaldos = await _dbContext.StockSaldos
            .Where(s => s.TenantId == tenantId && s.SucursalId == sucursalId && productIds.Contains(s.ProductoId))
            .ToListAsync(cancellationToken);

        var saldoByProduct = existingSaldos.ToDictionary(s => s.ProductoId, s => s);

        var now = DateTimeOffset.UtcNow;
        var added = false;

        foreach (var product in products)
        {
            if (saldoByProduct.ContainsKey(product.Id))
            {
                continue;
            }

            var saldo = new StockSaldo(Guid.NewGuid(), tenantId, product.Id, sucursalId, 0m, now);
            _dbContext.StockSaldos.Add(saldo);
            saldoByProduct[product.Id] = saldo;
            added = true;
        }

        if (added)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var result = products.Select(p =>
        {
            var saldo = saldoByProduct[p.Id];
            return new StockSaldoDto(p.Id, p.Name, p.Sku, saldo.CantidadActual);
        }).ToList();

        return result;
    }

    public async Task<IReadOnlyList<StockAlertaDto>> GetAlertasAsync(
        Guid tenantId,
        Guid sucursalId,
        CancellationToken cancellationToken = default)
    {
        var configs = await (from c in _dbContext.ProductoStockConfigs.AsNoTracking()
                join p in _dbContext.Productos.AsNoTracking().Where(p => p.TenantId == tenantId)
                    on c.ProductoId equals p.Id
                join pp in _dbContext.ProductoProveedores.AsNoTracking()
                    on new { p.Id, p.TenantId } equals new { Id = pp.ProductoId, pp.TenantId } into ppJoin
                from pp in ppJoin.Where(x => x.EsPrincipal).DefaultIfEmpty()
                join pr in _dbContext.Proveedores.AsNoTracking().Where(pr => pr.TenantId == tenantId)
                    on pp.ProveedorId equals pr.Id into prov
                from pr in prov.DefaultIfEmpty()
                join s in _dbContext.StockSaldos.AsNoTracking()
                    on new { c.ProductoId, c.TenantId, c.SucursalId }
                    equals new { s.ProductoId, s.TenantId, s.SucursalId } into saldoJoin
                from saldo in saldoJoin.DefaultIfEmpty()
                where c.TenantId == tenantId && c.SucursalId == sucursalId
                select new
                {
                    p.Id,
                    p.Name,
                    p.Sku,
                    ProveedorId = pp != null ? (Guid?)pp.ProveedorId : null,
                    Proveedor = pr != null ? pr.Name : null,
                    StockActual = saldo != null ? saldo.CantidadActual : 0m,
                    c.StockMinimo,
                    c.ToleranciaPct
                })
            .ToListAsync(cancellationToken);

        var alertas = new List<StockAlertaDto>();
        foreach (var item in configs)
        {
            var nivel = item.StockActual <= item.StockMinimo
                ? "CRITICO"
                : item.StockActual <= item.StockMinimo * item.ToleranciaPct
                    ? "BAJO"
                    : null;

            if (nivel is null)
            {
                continue;
            }

            alertas.Add(new StockAlertaDto(
                item.Id,
                item.Name,
                item.Sku,
                item.StockActual,
                item.StockMinimo,
                item.ToleranciaPct,
                nivel));
        }

        return alertas.OrderBy(a => a.Nombre).ToList();
    }

    public async Task<StockSugeridoCompraDto> GetSugeridoCompraAsync(
        Guid tenantId,
        Guid sucursalId,
        CancellationToken cancellationToken = default)
    {
        var configs = await (from c in _dbContext.ProductoStockConfigs.AsNoTracking()
                join p in _dbContext.Productos.AsNoTracking().Where(p => p.TenantId == tenantId)
                    on c.ProductoId equals p.Id
                join pp in _dbContext.ProductoProveedores.AsNoTracking()
                    on new { p.Id, p.TenantId } equals new { Id = pp.ProductoId, pp.TenantId } into ppJoin
                from pp in ppJoin.Where(x => x.EsPrincipal).DefaultIfEmpty()
                join pr in _dbContext.Proveedores.AsNoTracking().Where(pr => pr.TenantId == tenantId)
                    on pp.ProveedorId equals pr.Id into prov
                from pr in prov.DefaultIfEmpty()
                join s in _dbContext.StockSaldos.AsNoTracking()
                    on new { c.ProductoId, c.TenantId, c.SucursalId }
                    equals new { s.ProductoId, s.TenantId, s.SucursalId } into saldoJoin
                from saldo in saldoJoin.DefaultIfEmpty()
                where c.TenantId == tenantId && c.SucursalId == sucursalId
                select new
                {
                    p.Id,
                    p.Name,
                    p.Sku,
                    ProveedorId = pp != null ? (Guid?)pp.ProveedorId : null,
                    Proveedor = pr != null ? pr.Name : null,
                    StockActual = saldo != null ? saldo.CantidadActual : 0m,
                    c.StockMinimo,
                    c.ToleranciaPct
                })
            .ToListAsync(cancellationToken);

        var candidates = configs
            .Where(x => x.StockActual <= x.StockMinimo * x.ToleranciaPct)
            .ToList();

        if (!candidates.Any())
        {
            return new StockSugeridoCompraDto(0m, 0, Array.Empty<StockSugeridoProveedorDto>());
        }

        var productIds = candidates.Select(c => c.Id).Distinct().ToList();
        var codeMap = await _dbContext.ProductoCodigos.AsNoTracking()
            .Where(c => c.TenantId == tenantId && productIds.Contains(c.ProductoId))
            .GroupBy(c => c.ProductoId)
            .Select(g => new { ProductoId = g.Key, Codigo = g.OrderBy(x => x.Codigo).Select(x => x.Codigo).FirstOrDefault() })
            .ToDictionaryAsync(x => x.ProductoId, x => x.Codigo, cancellationToken);

        var items = candidates.Select(c =>
        {
            codeMap.TryGetValue(c.Id, out var codigo);
            var codigoPrincipal = string.IsNullOrWhiteSpace(codigo) ? c.Sku : codigo!;
            var sugerido = Math.Max((c.StockMinimo * c.ToleranciaPct) - c.StockActual, 0m);
            return new
            {
                c.ProveedorId,
                Proveedor = string.IsNullOrWhiteSpace(c.Proveedor) ? "SIN PROVEEDOR" : c.Proveedor!,
                Item = new StockSugeridoItemDto(
                    c.Id,
                    c.Name,
                    c.Sku,
                    codigoPrincipal,
                    c.StockActual,
                    c.StockMinimo,
                    sugerido)
            };
        }).ToList();

        var grupos = items
            .GroupBy(i => new { i.ProveedorId, i.Proveedor })
            .Select(g =>
            {
                var list = g.Select(x => x.Item).OrderBy(x => x.Nombre).ToList();
                return new StockSugeridoProveedorDto(
                    g.Key.ProveedorId,
                    g.Key.Proveedor,
                    list.Sum(i => i.Sugerido),
                    list.Count(),
                    list);
            })
            .OrderBy(g => g.Proveedor)
            .ToList();

        return new StockSugeridoCompraDto(
            grupos.Sum(g => g.TotalSugerido),
            grupos.Sum(g => g.TotalItems),
            grupos);
    }
}
