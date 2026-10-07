using Microsoft.AspNetCore.Mvc;
using PaintTintCalculator.Application.DTOs.DispenseJobs;
using PaintTintCalculator.Application.Features.DispenseJobs.Commands;

namespace PaintTintCalculator.Api.Controllers;

[ApiController]
[Route("api/dispense-jobs")]
public class DispenseJobsController : ControllerBase
{
    private readonly CreateDispenseJobCommandHandler _createHandler;
    private readonly GetRecentDispenseJobsQueryHandler _recentHandler;

    public DispenseJobsController(
        CreateDispenseJobCommandHandler createHandler,
        GetRecentDispenseJobsQueryHandler recentHandler)
    {
        _createHandler = createHandler;
        _recentHandler = recentHandler;
    }

    [HttpPost]
    public async Task<ActionResult<DispenseJobDto>> Create(
        [FromBody] CreateDispenseJobRequest request,
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

        var result = await _createHandler.HandleAsync(request, cancellationToken);
        return Created($"/api/dispense-jobs/{result.Id}", result);
    }

    [HttpGet("recent")]
    public async Task<ActionResult<IReadOnlyList<DispenseJobDto>>> GetRecent(
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        var jobs = await _recentHandler.HandleAsync(count, cancellationToken);
        return Ok(jobs);
    }
}

