using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Application.DTOs.Etiquetas;
using Pos.Application.UseCases.Etiquetas;

namespace Pos.WebApi.Controllers;

[ApiController]
[Route("api/v1/etiquetas")]
public sealed class EtiquetasController : ControllerBase
{
    private readonly EtiquetasService _service;

    public EtiquetasController(EtiquetasService service)
    {
        _service = service;
    }

    [HttpPost("pdf")]
    [Authorize(Policy = "PERM_PRODUCTO_VER")]
    public async Task<IActionResult> GenerarPdf(
        [FromBody] EtiquetaRequestDto request,
        CancellationToken cancellationToken)
    {
        var pdf = await _service.GenerarPdfAsync(request, cancellationToken);
        return File(pdf, "application/pdf", "etiquetas.pdf");
    }
}
