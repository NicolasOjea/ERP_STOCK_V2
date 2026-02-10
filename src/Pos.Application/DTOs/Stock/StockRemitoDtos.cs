namespace Pos.Application.DTOs.Stock;

public sealed record StockRemitoItemDto(Guid ProductoId, decimal Cantidad);

public sealed record StockRemitoRequestDto(IReadOnlyCollection<StockRemitoItemDto> Items);

public sealed record StockRemitoProductoDto(
    Guid ProductoId,
    string Nombre,
    string Sku,
    string Codigo);

public sealed record StockRemitoPdfItemDto(
    string Nombre,
    string Codigo,
    decimal Cantidad);

public sealed record StockRemitoPdfDataDto(
    DateTimeOffset Fecha,
    IReadOnlyList<StockRemitoPdfItemDto> Items);
