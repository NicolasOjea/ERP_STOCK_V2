using Pos.Application.Abstractions;
using Pos.Application.DTOs.Etiquetas;
using Pos.Domain.Exceptions;

namespace Pos.Application.UseCases.Etiquetas;

public sealed class EtiquetasService
{
    private const string ListaPrecioMinorista = "Minorista";

    private readonly IProductRepository _productRepository;
    private readonly IEtiquetaPdfGenerator _pdfGenerator;
    private readonly IRequestContext _requestContext;

    public EtiquetasService(
        IProductRepository productRepository,
        IEtiquetaPdfGenerator pdfGenerator,
        IRequestContext requestContext)
    {
        _productRepository = productRepository;
        _pdfGenerator = pdfGenerator;
        _requestContext = requestContext;
    }

    public async Task<byte[]> GenerarPdfAsync(EtiquetaRequestDto request, CancellationToken cancellationToken)
    {
        if (request is null || request.ProductoIds is null || request.ProductoIds.Count == 0)
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["productoIds"] = new[] { "Debe incluir al menos un producto." }
                });
        }

        if (request.ProductoIds.Any(id => id == Guid.Empty))
        {
            throw new ValidationException(
                "Validacion fallida.",
                new Dictionary<string, string[]>
                {
                    ["productoIds"] = new[] { "Producto invalido en la lista." }
                });
        }

        var tenantId = EnsureTenant();
        var ids = request.ProductoIds.Distinct().ToList();

        var items = await _productRepository.GetLabelDataAsync(
            tenantId,
            ids,
            ListaPrecioMinorista,
            cancellationToken);

        if (items.Count != ids.Count)
        {
            throw new NotFoundException("Producto no encontrado.");
        }

        var data = new EtiquetaPdfDataDto(items);
        return _pdfGenerator.Generate(data);
    }

    private Guid EnsureTenant()
    {
        if (_requestContext.TenantId == Guid.Empty)
        {
            throw new UnauthorizedException("Contexto de tenant invalido.");
        }

        return _requestContext.TenantId;
    }
}
