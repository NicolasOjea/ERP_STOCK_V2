using Pos.Application.DTOs.Caja;
using Pos.Domain.Enums;

namespace Pos.Application.Abstractions;

public interface ICajaRepository
{
    Task<bool> CajaExistsAsync(Guid tenantId, Guid sucursalId, Guid cajaId, CancellationToken cancellationToken = default);
    Task<bool> HasOpenSessionAsync(Guid tenantId, Guid cajaId, CancellationToken cancellationToken = default);

    Task<CajaSesionDto> OpenSessionAsync(
        Guid tenantId,
        Guid sucursalId,
        Guid cajaId,
        decimal montoInicial,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);

    Task<CajaMovimientoResultDto> AddMovimientoAsync(
        Guid tenantId,
        Guid sucursalId,
        Guid cajaSesionId,
        CajaMovimientoTipo tipo,
        decimal montoSigned,
        string motivo,
        string medioPago,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);

    Task<CajaResumenDto?> GetResumenAsync(
        Guid tenantId,
        Guid sucursalId,
        Guid cajaSesionId,
        CancellationToken cancellationToken = default);

    Task<CajaSesionDto?> GetSesionAsync(
        Guid tenantId,
        Guid sucursalId,
        Guid cajaSesionId,
        CancellationToken cancellationToken = default);

    Task<CajaCierreResultDto> CloseSessionAsync(
        Guid tenantId,
        Guid sucursalId,
        Guid cajaSesionId,
        CajaCierreRequestDto request,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);
}
