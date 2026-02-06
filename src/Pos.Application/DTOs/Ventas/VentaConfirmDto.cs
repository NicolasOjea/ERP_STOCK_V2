using Pos.Application.DTOs.Caja;
using Pos.Application.DTOs.Stock;

namespace Pos.Application.DTOs.Ventas;

public sealed record VentaPagoRequestDto(string MedioPago, decimal Monto);

public sealed record VentaConfirmRequestDto(IReadOnlyCollection<VentaPagoRequestDto> Pagos);

public sealed record VentaPagoDto(Guid Id, string MedioPago, decimal Monto);

public sealed record VentaConfirmResultDto(
    VentaDto Venta,
    IReadOnlyCollection<VentaPagoDto> Pagos,
    IReadOnlyCollection<StockSaldoChangeDto> StockCambios,
    IReadOnlyCollection<CajaMovimientoResultDto> CajaMovimientos);
