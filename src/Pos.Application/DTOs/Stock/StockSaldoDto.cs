namespace Pos.Application.DTOs.Stock;

public sealed record StockSaldoDto(
    Guid ProductoId,
    string Nombre,
    string Sku,
    decimal CantidadActual);
