using FinanCore.Application.Abstractions;
using FinanCore.Application.DTOs.Categorias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanCore.API.Controllers;

[ApiController]
[Authorize]
[Route("api/categorias")]
public class CategoriasController(ICategoriaService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoriaResponse>>> Listar(CancellationToken cancellationToken)
        => Ok(await service.ListarAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
        => Ok(await service.ObterPorIdAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoriaResponse>> Criar(CriarCategoriaRequest request, CancellationToken cancellationToken)
    {
        var categoria = await service.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaResponse>> Atualizar(Guid id, AtualizarCategoriaRequest request, CancellationToken cancellationToken)
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
