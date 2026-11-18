using FinanCore.Application.Abstractions;
using FinanCore.Application.DTOs.Fornecedores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanCore.API.Controllers;

[ApiController]
[Authorize]
[Route("api/fornecedores")]
public class FornecedoresController(IFornecedorService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FornecedorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FornecedorResponse>>> Listar(CancellationToken cancellationToken)
        => Ok(await service.ListarAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FornecedorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FornecedorResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
        => Ok(await service.ObterPorIdAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(FornecedorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FornecedorResponse>> Criar(CriarFornecedorRequest request, CancellationToken cancellationToken)
    {
        var fornecedor = await service.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = fornecedor.Id }, fornecedor);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FornecedorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FornecedorResponse>> Atualizar(Guid id, AtualizarFornecedorRequest request, CancellationToken cancellationToken)
        => Ok(await service.AtualizarAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken cancellationToken)
    {
        await service.DesativarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/ativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ativar(Guid id, CancellationToken cancellationToken)
    {
        await service.AtivarAsync(id, cancellationToken);
        return NoContent();
    }
}
