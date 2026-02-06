namespace Pos.Application.DTOs.Caja;

public sealed record CajaSesionDto(
    Guid Id,
    Guid CajaId,
    Guid SucursalId,
    decimal MontoInicial,
    DateTimeOffset AperturaAt,
    string Estado);
