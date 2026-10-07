using Microsoft.AspNetCore.Mvc;
using PaintTintCalculator.Application.DTOs.Shades;
using PaintTintCalculator.Application.Features.Shades.Queries;

namespace PaintTintCalculator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShadesController : ControllerBase
{
    private readonly SearchShadesQueryHandler _searchHandler;
    private readonly GetShadeDetailsQueryHandler _detailsHandler;
    private readonly GetBasesQueryHandler _basesHandler;

    public ShadesController(
        SearchShadesQueryHandler searchHandler,
        GetShadeDetailsQueryHandler detailsHandler,
        GetBasesQueryHandler basesHandler)
    {
        _searchHandler = searchHandler;
        _detailsHandler = detailsHandler;
        _basesHandler = basesHandler;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShadeSummaryDto>>> Search(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var shades = await _searchHandler.HandleAsync(search, cancellationToken);
        return Ok(shades);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShadeDetailsDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new { code = "INVALID_ID", message = "Shade ID must be greater than zero." });
        }

        var shade = await _detailsHandler.HandleAsync(id, cancellationToken);
        if (shade == null)
        {
            return NotFound(new { code = "SHADE_NOT_FOUND", message = $"Shade with ID {id} was not found." });
        }

        return Ok(shade);
    }

    [HttpGet("bases")]
    public async Task<ActionResult<IReadOnlyList<BaseDto>>> GetBases(CancellationToken cancellationToken)
    {
        var bases = await _basesHandler.HandleAsync(cancellationToken);
        return Ok(bases);
    }
}
