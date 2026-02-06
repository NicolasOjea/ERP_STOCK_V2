namespace Pos.Application.DTOs.Stock;

public sealed record StockConfigUpdateDto(decimal? StockMinimo, decimal? ToleranciaPct);
