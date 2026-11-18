using FinanCore.Application.Abstractions;
using FinanCore.Application.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanCore.API.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController(IDashboardService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardResponse>> Obter(
        [FromQuery] DateTime? periodoInicio,
        [FromQuery] DateTime? periodoFim,
        CancellationToken cancellationToken)
        => Ok(await service.ObterAsync(periodoInicio, periodoFim, cancellationToken));
}