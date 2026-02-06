namespace Pos.Application.DTOs.Stock;

public sealed record StockAlertaDto(
    Guid ProductoId,
    string Nombre,
    string Sku,
    decimal CantidadActual,
    decimal StockMinimo,
    decimal ToleranciaPct,
    string Nivel);
