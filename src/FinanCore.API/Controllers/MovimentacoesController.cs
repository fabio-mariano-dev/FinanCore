using FinanCore.Application.Abstractions;
using FinanCore.Application.DTOs.Movimentacoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanCore.API.Controllers;

[ApiController]
[Authorize]
[Route("api/movimentacoes")]
public class MovimentacoesController(IMovimentacaoFinanceiraService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MovimentacaoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MovimentacaoResponse>>> Listar([FromQuery] MovimentacaoFiltro filtro, CancellationToken cancellationToken)
        => Ok(await service.ListarAsync(filtro, cancellationToken));

    [HttpGet("fluxo-caixa")]
    [ProducesResponseType(typeof(FluxoCaixaResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FluxoCaixaResponse>> ObterFluxoCaixa(
        [FromQuery] DateTime periodoInicio,
        [FromQuery] DateTime periodoFim,
        CancellationToken cancellationToken)
        => Ok(await service.ObterFluxoCaixaAsync(periodoInicio, periodoFim, cancellationToken));
}
