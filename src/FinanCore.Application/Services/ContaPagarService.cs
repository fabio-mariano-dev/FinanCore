using FinanCore.Application.Abstractions;
using FinanCore.Application.DTOs.ContasPagar;
using FinanCore.Domain.Entities;
using FinanCore.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.Application.Services;

public class ContaPagarService(
    IFinanCoreDbContext dbContext,
    IValidator<CriarContaPagarRequest> criarValidator,
    IValidator<AtualizarContaPagarRequest> atualizarValidator,
    IValidator<RegistrarPagamentoRequest> pagamentoValidator) : IContaPagarService
{
    public async Task<ContaPagarResponse> CriarAsync(CriarContaPagarRequest request, CancellationToken cancellationToken = default)
    {
        await criarValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GarantirReferenciasExistemAsync(request.FornecedorId, request.CategoriaId, cancellationToken);

        var contaPagar = ContaPagar.Criar(
            request.FornecedorId,
            request.CategoriaId,
            request.Descricao,
            request.Valor,
            request.DataEmissao,
            request.DataVencimento,
            request.Observacoes);

        dbContext.ContasPagar.Add(contaPagar);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ParaResponse(contaPagar);
    }

    public async Task<ContaPagarResponse> AtualizarAsync(Guid id, AtualizarContaPagarRequest request, CancellationToken cancellationToken = default)
    {
        await atualizarValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GarantirReferenciasExistemAsync(request.FornecedorId, request.CategoriaId, cancellationToken);

        var contaPagar = await ObterEntidadeAsync(id, cancellationToken);

        contaPagar.Atualizar(request.FornecedorId, request.CategoriaId, request.Descricao, request.Valor, request.DataVencimento, request.Observacoes);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ParaResponse(contaPagar);
    }

    public async Task<ContaPagarResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contaPagar = await ObterEntidadeAsync(id, cancellationToken);
        return ParaResponse(contaPagar);
    }

    public async Task<IReadOnlyList<ContaPagarResponse>> ListarAsync(ContaPagarFiltro? filtro = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ContasPagar.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(filtro?.Status)) query = query.Where(c => c.Status.ToString() == filtro.Status);
        if (filtro?.FornecedorId is Guid fornecedorId) query = query.Where(c => c.FornecedorId == fornecedorId);
        if (filtro?.VencimentoInicio is DateTime inicio) query = query.Where(c => c.DataVencimento >= inicio);
        if (filtro?.VencimentoFim is DateTime fim) query = query.Where(c => c.DataVencimento <= fim);
        var contas = await query
            .OrderBy(c => c.DataVencimento)
            .ToListAsync(cancellationToken);

        return contas.Select(ParaResponse).ToList();
    }

    public async Task<ContaPagarResponse> RegistrarPagamentoAsync(Guid id, RegistrarPagamentoRequest request, CancellationToken cancellationToken = default)
    {
        await pagamentoValidator.ValidateAndThrowAsync(request, cancellationToken);

        var contaPagar = await ObterEntidadeAsync(id, cancellationToken);
        if (await dbContext.MovimentacoesFinanceiras.AnyAsync(m => m.ContaPagarId == id, cancellationToken))
            throw new RegraNegocioException("Já existe uma movimentação para esta conta a pagar.");

        contaPagar.RegistrarPagamento(request.DataPagamento);

        var movimentacao = MovimentacaoFinanceira.ParaContaPagar(contaPagar.Id, contaPagar.CategoriaId, contaPagar.Valor, request.DataPagamento);
        dbContext.MovimentacoesFinanceiras.Add(movimentacao);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ParaResponse(contaPagar);
    }

    public async Task CancelarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contaPagar = await ObterEntidadeAsync(id, cancellationToken);

        contaPagar.Cancelar();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task GarantirReferenciasExistemAsync(Guid fornecedorId, Guid categoriaId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Fornecedores.AnyAsync(f => f.Id == fornecedorId && f.Ativo, cancellationToken))
            throw new RegraNegocioException($"Fornecedor '{fornecedorId}' não existe.");

        if (!await dbContext.Categorias.AnyAsync(c => c.Id == categoriaId && c.Ativo && c.Tipo == Domain.Enums.TipoCategoria.Despesa, cancellationToken))
            throw new RegraNegocioException($"Categoria '{categoriaId}' não existe.");
    }

    private async Task<ContaPagar> ObterEntidadeAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.ContasPagar.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Conta a pagar '{id}' não encontrada.");
    }

    private static ContaPagarResponse ParaResponse(ContaPagar contaPagar)
        => new(
            contaPagar.Id,
            contaPagar.FornecedorId,
            contaPagar.CategoriaId,
            contaPagar.Descricao,
            contaPagar.Valor,
            contaPagar.DataEmissao,
            contaPagar.DataVencimento,
            contaPagar.DataPagamento,
            contaPagar.Status.ToString(),
            contaPagar.Observacoes);
}
