using Pos.Application.DTOs.Reportes;

namespace Pos.Application.Abstractions;

public interface IReportesRepository
{
    Task<IReadOnlyList<VentaPorDiaItemDto>> GetVentasPorDiaAsync(
        Guid tenantId,
        Guid sucursalId,
        DateTimeOffset? desde,
        DateTimeOffset? hasta,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MedioPagoItemDto>> GetMediosPagoAsync(
        Guid tenantId,
        Guid sucursalId,
        DateTimeOffset? desde,
        DateTimeOffset? hasta,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopProductoItemDto>> GetTopProductosAsync(
        Guid tenantId,
        Guid sucursalId,
        DateTimeOffset? desde,
        DateTimeOffset? hasta,
        int top,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RotacionStockItemDto>> GetRotacionStockAsync(
        Guid tenantId,
        Guid sucursalId,
        DateTimeOffset? desde,
        DateTimeOffset? hasta,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockInmovilizadoItemDto>> GetStockInmovilizadoAsync(
        Guid tenantId,
        Guid sucursalId,
        int dias,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);
}
