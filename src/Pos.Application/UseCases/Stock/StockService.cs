using System.Text.Json;
using Pos.Application.Abstractions;
using Pos.Application.DTOs.Stock;
using Pos.Domain.Enums;
using Pos.Domain.Exceptions;

namespace Pos.Application.UseCases.Stock;

public sealed class StockService
{
    private const decimal DefaultToleranciaPct = 25m;

    private readonly IStockRepository _stockRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IRemitoPdfGenerator _remitoPdfGenerator;
    private readonly IRequestContext _requestContext;
    private readonly IAuditLogService _auditLogService;

    public StockService(
        IStockRepository stockRepository,
        IStockMovementRepository stockMovementRepository,
        IRemitoPdfGenerator remitoPdfGenerator,
        IRequestContext requestContext,
        IAuditLogService auditLogService)
    {
        _stockRepository = stockRepository;
        _stockMovementRepository = stockMovementRepository;
        _remitoPdfGenerator = remitoPdfGenerator;
        _requestContext = requestContext;
        _auditLogService = auditLogService;
    }

    public async Task<StockConfigDto> UpdateStockConfigAsync(
        Guid productId,
        StockConfigUpdateDto request,
        CancellationToken cancellationToken)
    {
        if (productId == Guid.Empty)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["productId"] = new[] { "El producto es obligatorio." }
                });
        }

        if (request is null)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["request"] = new[] { "El request es obligatorio." }
                });
        }

        var hasChange = request.StockMinimo.HasValue || request.StockDeseado.HasValue || request.ToleranciaPct.HasValue;
        if (!hasChange)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["request"] = new[] { "Debe enviar al menos un cambio." }
                });
        }

        if (request.StockMinimo.HasValue && request.StockMinimo.Value < 0)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["stockMinimo"] = new[] { "El stock minimo no puede ser negativo." }
                });
        }

        if (request.StockDeseado.HasValue && request.StockDeseado.Value < 0)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["stockDeseado"] = new[] { "El stock deseado no puede ser negativo." }
                });
        }

        if (request.ToleranciaPct.HasValue && request.ToleranciaPct.Value < 0)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["toleranciaPct"] = new[] { "La tolerancia no puede ser negativa." }
                });
        }

        var tenantId = EnsureTenant();
        var sucursalId = EnsureSucursal();

        var productExists = await _stockRepository.ProductExistsAsync(tenantId, productId, cancellationToken);
        if (!productExists)
        {
            throw new NotFoundException("Producto no encontrado.");
        }

        var before = await _stockRepository.GetStockConfigAsync(tenantId, sucursalId, productId, cancellationToken);

        if (before is null && !request.StockMinimo.HasValue)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["stockMinimo"] = new[] { "El stock minimo es obligatorio." }
                });
        }

        var stockMinimo = request.StockMinimo ?? before?.StockMinimo ?? 0m;
        var stockDeseado = request.StockDeseado ?? before?.StockDeseado ?? stockMinimo;
        var tolerancia = request.ToleranciaPct ?? before?.ToleranciaPct ?? DefaultToleranciaPct;

        var updated = await _stockRepository.UpsertStockConfigAsync(
            tenantId,
            sucursalId,
            productId,
            stockMinimo,
            stockDeseado,
            tolerancia,
            DateTimeOffset.UtcNow,
            cancellationToken);

        await _auditLogService.LogAsync(
            "ProductoStockConfig",
            productId.ToString(),
            AuditAction.Update,
            before is null ? null : JsonSerializer.Serialize(before),
            JsonSerializer.Serialize(updated),
            JsonSerializer.Serialize(new { sucursalId }),
            cancellationToken);

        return updated;
    }

    public async Task<IReadOnlyList<StockSaldoDto>> GetSaldosAsync(string? search, CancellationToken cancellationToken)
    {
        var tenantId = EnsureTenant();
        var sucursalId = EnsureSucursal();
        return await _stockRepository.GetSaldosAsync(tenantId, sucursalId, search, cancellationToken);
    }

    public async Task<StockConfigDto> GetStockConfigAsync(Guid productId, CancellationToken cancellationToken)
    {
        if (productId == Guid.Empty)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["productId"] = new[] { "El producto es obligatorio." }
                });
        }

        var tenantId = EnsureTenant();
        var sucursalId = EnsureSucursal();

        var config = await _stockRepository.GetStockConfigAsync(tenantId, sucursalId, productId, cancellationToken);
        return config ?? new StockConfigDto(productId, sucursalId, 0m, 0m, DefaultToleranciaPct);
    }

    public async Task<IReadOnlyList<StockAlertaDto>> GetAlertasAsync(CancellationToken cancellationToken)
    {
        var tenantId = EnsureTenant();
        var sucursalId = EnsureSucursal();
        return await _stockRepository.GetAlertasAsync(tenantId, sucursalId, cancellationToken);
    }

    public async Task<StockSugeridoCompraDto> GetSugeridoCompraAsync(CancellationToken cancellationToken)
    {
        var tenantId = EnsureTenant();
        var sucursalId = EnsureSucursal();
        return await _stockRepository.GetSugeridoCompraAsync(tenantId, sucursalId, cancellationToken);
    }

    public async Task<byte[]> GenerarRemitoAlertasAsync(
        StockRemitoRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request is null || request.Items is null || request.Items.Count == 0)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["items"] = new[] { "Debe incluir items." }
                });
        }

        foreach (var item in request.Items)
        {
            if (item.ProductoId == Guid.Empty)
            {
                throw new ValidationException(
                    "Validacion fallida.",
                    new Dictionary<string, string[]>
                    {
                        ["items"] = new[] { "Producto invalido en items." }
                    });
            }

            if (item.Cantidad <= 0)
            {
                throw new ValidationException(
                    "Validacion fallida.",
                    new Dictionary<string, string[]>
                    {
                        ["items"] = new[] { "Cantidad debe ser mayor a 0." }
                    });
            }
        }

        var tenantId = EnsureTenant();

        var ids = request.Items.Select(i => i.ProductoId).Distinct().ToList();
        var productos = await _stockRepository.GetProductosRemitoAsync(tenantId, ids, cancellationToken);
        if (productos.Count == 0)
        {
            throw new NotFoundException("No se encontraron productos para el remito.");
        }

        var productoMap = productos.ToDictionary(p => p.ProductoId, p => p);
        var pdfItems = request.Items
            .Where(i => productoMap.ContainsKey(i.ProductoId))
            .Select(i =>
            {
                var p = productoMap[i.ProductoId];
                return new StockRemitoPdfItemDto(p.Nombre, p.Codigo, i.Cantidad);
            })
            .OrderBy(i => i.Nombre)
            .ToList();

        var data = new StockRemitoPdfDataDto(DateTimeOffset.UtcNow, pdfItems);
        return _remitoPdfGenerator.Generate(data);
    }

    public async Task<StockMovimientoDto> RegistrarMovimientoAsync(
        StockMovimientoCreateDto request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["request"] = new[] { "El request es obligatorio." }
                });
        }

        if (string.IsNullOrWhiteSpace(request.Motivo))
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["motivo"] = new[] { "El motivo es obligatorio." }
                });
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["items"] = new[] { "Debe especificar al menos un item." }
                });
        }

        if (!Enum.TryParse<StockMovimientoTipo>(request.Tipo, true, out var tipo))
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["tipo"] = new[] { "Tipo de movimiento invalido." }
                });
        }

        foreach (var item in request.Items)
        {
            if (item.ProductoId == Guid.Empty)
            {
                throw new ValidationException(
                    "Validacion fallida.",
                    new Dictionary<string, string[]>
                    {
                        ["items"] = new[] { "Producto invalido en items." }
                    });
            }

            if (item.Cantidad <= 0)
            {
                throw new ValidationException(
                    "Validacion fallida.",
                    new Dictionary<string, string[]>
                    {
                        ["items"] = new[] { "Cantidad debe ser mayor a 0." }
                    });
            }

            if (tipo == StockMovimientoTipo.Merma && item.EsIngreso)
            {
                throw new ValidationException(
                    "Validacion fallida.",
                    new Dictionary<string, string[]>
                    {
                        ["items"] = new[] { "En merma solo se permite egreso." }
                    });
            }
        }

        var tenantId = EnsureTenant();
        var sucursalId = EnsureSucursal();

        var result = await _stockMovementRepository.RegisterAsync(
            tenantId,
            sucursalId,
            tipo,
            request.Motivo.Trim(),
            request.Items,
            DateTimeOffset.UtcNow,
            cancellationToken);

        foreach (var cambio in result.Cambios)
        {
            await _auditLogService.LogAsync(
                "StockSaldo",
                $"{cambio.ProductoId}:{sucursalId}",
                AuditAction.Adjust,
                JsonSerializer.Serialize(new { cantidadActual = cambio.SaldoAntes }),
                JsonSerializer.Serialize(new { cantidadActual = cambio.SaldoDespues }),
                JsonSerializer.Serialize(new { movimientoId = cambio.MovimientoId, itemId = cambio.MovimientoItemId }),
                cancellationToken);
        }

        return result.Movimiento;
    }

    public async Task<IReadOnlyList<StockMovimientoDto>> GetMovimientosAsync(
        Guid? productoId,
        DateTimeOffset? desde,
        DateTimeOffset? hasta,
        CancellationToken cancellationToken)
    {
        var tenantId = EnsureTenant();
        var sucursalId = EnsureSucursal();

        if (productoId.HasValue && productoId.Value == Guid.Empty)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["productoId"] = new[] { "El producto es invalido." }
                });
        }

        if (desde.HasValue && hasta.HasValue && desde > hasta)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["fecha"] = new[] { "El rango de fechas es invalido." }
                });
        }

        return await _stockMovementRepository.SearchAsync(tenantId, sucursalId, productoId, desde, hasta, cancellationToken);
    }

    private Guid EnsureTenant()
    {
        if (_requestContext.TenantId == Guid.Empty)
        {
            throw new UnauthorizedException("Contexto de tenant invalido.");
        }

        return _requestContext.TenantId;
    }

    private Guid EnsureSucursal()
    {
        if (_requestContext.SucursalId == Guid.Empty)
        {
            throw new UnauthorizedException("Contexto de sucursal invalido.");
        }

        return _requestContext.SucursalId;
    }
}
