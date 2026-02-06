using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pos.WebApi.Controllers;

[ApiController]
[Route("api/v1/stock")]
public sealed class StockController : ControllerBase
{
    [Authorize(Policy = "PERM_STOCK_AJUSTAR")]
    [HttpPost("adjust")]
    public ActionResult<object> Adjust([FromBody] StockAdjustRequest request)
    {
        return Ok(new { status = "ok" });
    }
}

public sealed record StockAdjustRequest(Guid ProductId, decimal Quantity);
