using FinanCore.Application.Abstractions;
using FinanCore.Application.DTOs.ContasPagar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanCore.API.Controllers;

[ApiController]
[Authorize]
[Route("api/contas-pagar")]
public class ContasPagarController(IContaPagarService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ContaPagarResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContaPagarResponse>>> Listar([FromQuery] ContaPagarFiltro filtro, CancellationToken cancellationToken)
        => Ok(await service.ListarAsync(filtro, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContaPagarResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContaPagarResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
        => Ok(await service.ObterPorIdAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(ContaPagarResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContaPagarResponse>> Criar(CriarContaPagarRequest request, CancellationToken cancellationToken)
    {
        var conta = await service.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = conta.Id }, conta);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContaPagarResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContaPagarResponse>> Atualizar(Guid id, AtualizarContaPagarRequest request, CancellationToken cancellationToken)
        => Ok(await service.AtualizarAsync(id, request, cancellationToken));

    [HttpPatch("{id:guid}/pagar")]
    [ProducesResponseType(typeof(ContaPagarResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContaPagarResponse>> RegistrarPagamento(Guid id, RegistrarPagamentoRequest request, CancellationToken cancellationToken)
        => Ok(await service.RegistrarPagamentoAsync(id, request, cancellationToken));

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
