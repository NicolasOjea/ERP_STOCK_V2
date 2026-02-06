using Pos.Domain.Common;
using Pos.Domain.Enums;

namespace Pos.Domain.Entities;

public sealed class CajaSesion : EntityBase
{
    private CajaSesion()
    {
    }

    public CajaSesion(
        Guid id,
        Guid tenantId,
        Guid cajaId,
        Guid sucursalId,
        decimal montoInicial,
        DateTimeOffset aperturaAt,
        DateTimeOffset createdAtUtc)
        : base(id, tenantId, createdAtUtc)
    {
        if (cajaId == Guid.Empty) throw new ArgumentException("CajaId is required.", nameof(cajaId));
        if (sucursalId == Guid.Empty) throw new ArgumentException("SucursalId is required.", nameof(sucursalId));

        CajaId = cajaId;
        SucursalId = sucursalId;
        MontoInicial = montoInicial;
        AperturaAt = aperturaAt;
        Estado = CajaSesionEstado.Abierta;
    }

    public Guid CajaId { get; private set; }
    public Guid SucursalId { get; private set; }
    public decimal MontoInicial { get; private set; }
    public decimal? MontoCierre { get; private set; }
    public decimal DiferenciaTotal { get; private set; }
    public string? MotivoDiferencia { get; private set; }
    public string? ArqueoJson { get; private set; }
    public DateTimeOffset AperturaAt { get; private set; }
    public DateTimeOffset? CierreAt { get; private set; }
    public CajaSesionEstado Estado { get; private set; }

    public void Cerrar(decimal montoCierre, decimal diferenciaTotal, string? motivoDiferencia, string? arqueoJson, DateTimeOffset cierreAtUtc)
    {
        MontoCierre = montoCierre;
        DiferenciaTotal = diferenciaTotal;
        MotivoDiferencia = string.IsNullOrWhiteSpace(motivoDiferencia) ? null : motivoDiferencia;
        ArqueoJson = string.IsNullOrWhiteSpace(arqueoJson) ? null : arqueoJson;
        CierreAt = cierreAtUtc;
        Estado = CajaSesionEstado.Cerrada;
        MarkUpdated(cierreAtUtc);
    }
}
