using FinanCore.Application.Abstractions;
using FinanCore.Application.DTOs.Categorias;
using FinanCore.Domain.Entities;
using FinanCore.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.Application.Services;

public class CategoriaService(
    IFinanCoreDbContext dbContext,
    IValidator<CriarCategoriaRequest> criarValidator,
    IValidator<AtualizarCategoriaRequest> atualizarValidator) : ICategoriaService
{
    public async Task<CategoriaResponse> CriarAsync(CriarCategoriaRequest request, CancellationToken cancellationToken = default)
    {
        await criarValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (await ExisteDuplicadaAsync(request.Nome, request.Tipo, null, cancellationToken))
            throw new RegraNegocioException("Já existe uma categoria com o mesmo nome e tipo.");

        var categoria = new Categoria(request.Nome, request.Tipo);

        dbContext.Categorias.Add(categoria);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ParaResponse(categoria);
    }

    public async Task<CategoriaResponse> AtualizarAsync(Guid id, AtualizarCategoriaRequest request, CancellationToken cancellationToken = default)
    {
        await atualizarValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (await ExisteDuplicadaAsync(request.Nome, request.Tipo, id, cancellationToken))
            throw new RegraNegocioException("Já existe uma categoria com o mesmo nome e tipo.");

        var categoria = await ObterEntidadeAsync(id, cancellationToken);

        categoria.Atualizar(request.Nome, request.Tipo);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ParaResponse(categoria);
    }

    public async Task<CategoriaResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var categoria = await ObterEntidadeAsync(id, cancellationToken);
        return ParaResponse(categoria);
    }

    public async Task<IReadOnlyList<CategoriaResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaResponse(c.Id, c.Nome, c.Tipo, c.Ativo))
            .ToListAsync(cancellationToken);
    }

    public async Task DesativarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var categoria = await ObterEntidadeAsync(id, cancellationToken);

        categoria.Desativar();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AtivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var categoria = await ObterEntidadeAsync(id, cancellationToken);

        categoria.Ativar();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task<bool> ExisteDuplicadaAsync(string nome, Domain.Enums.TipoCategoria tipo, Guid? id, CancellationToken cancellationToken)
    {
        var nomeNormalizado = nome.Trim();
        return dbContext.Categorias.AnyAsync(c =>
            c.Nome == nomeNormalizado && c.Tipo == tipo && (!id.HasValue || c.Id != id.Value), cancellationToken);
    }

    private async Task<Categoria> ObterEntidadeAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Categoria '{id}' não encontrada.");
    }

    private static CategoriaResponse ParaResponse(Categoria categoria)
        => new(categoria.Id, categoria.Nome, categoria.Tipo, categoria.Ativo);
}
