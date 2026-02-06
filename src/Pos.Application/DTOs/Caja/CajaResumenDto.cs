namespace Pos.Application.DTOs.Caja;

public sealed record CajaResumenDto(
    Guid CajaSesionId,
    Guid CajaId,
    decimal MontoInicial,
    decimal TotalIngresos,
    decimal TotalEgresos,
    decimal SaldoActual,
    int TotalMovimientos);
