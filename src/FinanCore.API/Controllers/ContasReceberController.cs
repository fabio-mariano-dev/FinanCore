using FinanCore.Application.Abstractions;
using FinanCore.Application.DTOs.ContasReceber;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanCore.API.Controllers;

[ApiController]
[Authorize]
[Route("api/contas-receber")]
public class ContasReceberController(IContaReceberService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ContaReceberResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContaReceberResponse>>> Listar([FromQuery] ContaReceberFiltro filtro, CancellationToken cancellationToken)
        => Ok(await service.ListarAsync(filtro, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContaReceberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContaReceberResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
        => Ok(await service.ObterPorIdAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(ContaReceberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContaReceberResponse>> Criar(CriarContaReceberRequest request, CancellationToken cancellationToken)
    {
        var conta = await service.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = conta.Id }, conta);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContaReceberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContaReceberResponse>> Atualizar(Guid id, AtualizarContaReceberRequest request, CancellationToken cancellationToken)
        => Ok(await service.AtualizarAsync(id, request, cancellationToken));

    [HttpPatch("{id:guid}/receber")]
    [ProducesResponseType(typeof(ContaReceberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContaReceberResponse>> RegistrarRecebimento(Guid id, RegistrarRecebimentoRequest request, CancellationToken cancellationToken)
        => Ok(await service.RegistrarRecebimentoAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        await service.CancelarAsync(id, cancellationToken);
        return NoContent();
    }
}
