using Microsoft.EntityFrameworkCore;
using Pos.Application.Abstractions;
using Pos.Application.DTOs.Reportes;
using Pos.Domain.Enums;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Repositories;

public sealed class ReportesRepository : IReportesRepository
{
    private readonly PosDbContext _dbContext;

    public ReportesRepository(PosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<VentaPorDiaItemDto>> GetVentasPorDiaAsync(
        Guid tenantId,
        Guid sucursalId,
        DateTimeOffset? desde,
        DateTimeOffset? hasta,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Ventas.AsNoTracking()
            .Where(v => v.TenantId == tenantId && v.SucursalId == sucursalId && v.Estado == VentaEstado.Confirmada);

        if (desde.HasValue)
        {
            query = query.Where(v => v.UpdatedAt >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(v => v.UpdatedAt <= hasta.Value);
        }

        var result = await query
            .GroupBy(v => v.UpdatedAt.Date)
            .Select(g => new VentaPorDiaItemDto(g.Key, g.Sum(x => x.TotalNeto)))
            .OrderBy(x => x.Fecha)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<MedioPagoItemDto>> GetMediosPagoAsync(
        Guid tenantId,
        Guid sucursalId,
        DateTimeOffset? desde,
        DateTimeOffset? hasta,
        CancellationToken cancellationToken = default)
    {
        var query = from pago in _dbContext.VentaPagos.AsNoTracking()
            join venta in _dbContext.Ventas.AsNoTracking() on pago.VentaId equals venta.Id
            where pago.TenantId == tenantId
                  && venta.TenantId == tenantId
                  && venta.SucursalId == sucursalId
                  && venta.Estado == VentaEstado.Confirmada
            select new { pago.MedioPago, pago.Monto, venta.UpdatedAt };

        if (desde.HasValue)
        {
            query = query.Where(x => x.UpdatedAt >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(x => x.UpdatedAt <= hasta.Value);
        }

        var result = await query
            .GroupBy(x => x.MedioPago)
            .Select(g => new MedioPagoItemDto(g.Key, g.Sum(x => x.Monto)))
            .OrderByDescending(x => x.Total)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<TopProductoItemDto>> GetTopProductosAsync(
        Guid tenantId,
        Guid sucursalId,
        DateTimeOffset? desde,
        DateTimeOffset? hasta,
        int top,
        CancellationToken cancellationToken = default)
    {
        var query = from item in _dbContext.VentaItems.AsNoTracking()
            join venta in _dbContext.Ventas.AsNoTracking() on item.VentaId equals venta.Id
            join producto in _dbContext.Productos.AsNoTracking() on item.ProductoId equals producto.Id
            where item.TenantId == tenantId
                  && venta.TenantId == tenantId
                  && venta.SucursalId == sucursalId
                  && venta.Estado == VentaEstado.Confirmada
                  && producto.TenantId == tenantId
            select new
            {
                item.ProductoId,
                producto.Name,
                producto.Sku,
                item.Cantidad,
                item.PrecioUnitario,
                venta.UpdatedAt
            };

        if (desde.HasValue)
        {
            query = query.Where(x => x.UpdatedAt >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(x => x.UpdatedAt <= hasta.Value);
        }

        var result = await query
            .GroupBy(x => new { x.ProductoId, x.Name, x.Sku })
            .Select(g => new TopProductoItemDto(
                g.Key.ProductoId,
                g.Key.Name,
                g.Key.Sku,
                g.Sum(x => x.Cantidad),
                g.Sum(x => x.Cantidad * x.PrecioUnitario)))
            .OrderByDescending(x => x.Total)
            .Take(top)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<RotacionStockItemDto>> GetRotacionStockAsync(
        Guid tenantId,
        Guid sucursalId,
        DateTimeOffset? desde,
        DateTimeOffset? hasta,
        CancellationToken cancellationToken = default)
    {
        var query = from item in _dbContext.StockMovimientoItems.AsNoTracking()
            join mov in _dbContext.StockMovimientos.AsNoTracking() on item.MovimientoId equals mov.Id
            join producto in _dbContext.Productos.AsNoTracking() on item.ProductoId equals producto.Id
            where item.TenantId == tenantId
                  && mov.TenantId == tenantId
                  && mov.SucursalId == sucursalId
                  && producto.TenantId == tenantId
            select new
            {
                item.ProductoId,
                producto.Name,
                producto.Sku,
                item.Cantidad,
                item.EsIngreso,
                mov.Fecha
            };

        if (desde.HasValue)
        {
            query = query.Where(x => x.Fecha >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(x => x.Fecha <= hasta.Value);
        }

        var result = await query
            .GroupBy(x => new { x.ProductoId, x.Name, x.Sku })
            .Select(g => new RotacionStockItemDto(
                g.Key.ProductoId,
                g.Key.Name,
                g.Key.Sku,
                g.Where(x => x.EsIngreso).Sum(x => x.Cantidad),
                g.Where(x => !x.EsIngreso).Sum(x => x.Cantidad),
                g.Where(x => x.EsIngreso).Sum(x => x.Cantidad) - g.Where(x => !x.EsIngreso).Sum(x => x.Cantidad)))
            .OrderByDescending(x => x.Salidas)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<StockInmovilizadoItemDto>> GetStockInmovilizadoAsync(
        Guid tenantId,
        Guid sucursalId,
        int dias,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var limite = nowUtc.AddDays(-dias);

        var lastMovimientos = from item in _dbContext.StockMovimientoItems.AsNoTracking()
            join mov in _dbContext.StockMovimientos.AsNoTracking() on item.MovimientoId equals mov.Id
            where item.TenantId == tenantId
                  && mov.TenantId == tenantId
                  && mov.SucursalId == sucursalId
            group mov by item.ProductoId
            into g
            select new
            {
                ProductoId = g.Key,
                UltimoMovimiento = g.Max(x => x.Fecha)
            };

        var saldos = from saldo in _dbContext.StockSaldos.AsNoTracking()
            join producto in _dbContext.Productos.AsNoTracking() on saldo.ProductoId equals producto.Id
            where saldo.TenantId == tenantId
                  && saldo.SucursalId == sucursalId
                  && saldo.CantidadActual > 0
                  && producto.TenantId == tenantId
            select new
            {
                saldo.ProductoId,
                saldo.CantidadActual,
                producto.Name,
                producto.Sku,
                producto.CreatedAt
            };

        var query = from s in saldos
            join lm in lastMovimientos on s.ProductoId equals lm.ProductoId into lmGroup
            from lm in lmGroup.DefaultIfEmpty()
            where (lm == null && s.CreatedAt <= limite)
                || (lm != null && lm.UltimoMovimiento <= limite)
            select new
            {
                s.ProductoId,
                s.Name,
                s.Sku,
                s.CantidadActual,
                UltimoMovimiento = lm == null ? (DateTimeOffset?)null : lm.UltimoMovimiento,
                s.CreatedAt
            };

        var data = await query.ToListAsync(cancellationToken);

        var result = data
            .Select(item =>
            {
                var baseDate = item.UltimoMovimiento ?? item.CreatedAt;
                var diasSinMovimiento = (int)Math.Floor((nowUtc - baseDate).TotalDays);
                return new StockInmovilizadoItemDto(
                    item.ProductoId,
                    item.Name,
                    item.Sku,
                    item.CantidadActual,
                    item.UltimoMovimiento,
                    diasSinMovimiento);
            })
            .OrderByDescending(x => x.DiasSinMovimiento)
            .ToList();

        return result;
    }
}
