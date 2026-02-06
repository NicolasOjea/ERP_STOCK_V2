namespace Pos.Application.DTOs.Ventas;

public sealed record VentaDto(
    Guid Id,
    Guid SucursalId,
    Guid? UserId,
    string Estado,
    string ListaPrecio,
    decimal TotalNeto,
    decimal TotalPagos,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<VentaItemDto> Items);
