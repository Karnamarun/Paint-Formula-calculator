using Microsoft.AspNetCore.Mvc;
using PaintTintCalculator.Application.DTOs.Tint;
using PaintTintCalculator.Application.Features.TintCalculation.Commands;

namespace PaintTintCalculator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TintController : ControllerBase
{
    private readonly CalculateTintCommandHandler _calculateHandler;

    public TintController(CalculateTintCommandHandler calculateHandler)
    {
        _calculateHandler = calculateHandler;
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<TintCalculationResultDto>> Calculate(
        [FromBody] CalculateTintRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ShadeId <= 0)
        {
            return BadRequest(new { code = "INVALID_SHADE_ID", message = "ShadeId must be greater than zero." });
        }

        if (request.BaseId <= 0)
        {
            return BadRequest(new { code = "INVALID_BASE_ID", message = "BaseId must be greater than zero." });
        }

        if (request.CanSizeLitres <= 0)
        {
            return BadRequest(new { code = "INVALID_CAN_SIZE", message = "CanSizeLitres must be greater than zero." });
        }

        var result = await _calculateHandler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }
}

