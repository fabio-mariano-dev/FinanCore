using FinanCore.Application.Abstractions;
using FinanCore.Application.DTOs.ContasReceber;
using FinanCore.Domain.Entities;
using FinanCore.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.Application.Services;

public class ContaReceberService(
    IFinanCoreDbContext dbContext,
    IValidator<CriarContaReceberRequest> criarValidator,
    IValidator<AtualizarContaReceberRequest> atualizarValidator,
    IValidator<RegistrarRecebimentoRequest> recebimentoValidator) : IContaReceberService
{
    public async Task<ContaReceberResponse> CriarAsync(CriarContaReceberRequest request, CancellationToken cancellationToken = default)
    {
        await criarValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GarantirReferenciasExistemAsync(request.ClienteId, request.CategoriaId, cancellationToken);

        var contaReceber = ContaReceber.Criar(
            request.ClienteId,
            request.CategoriaId,
            request.Descricao,
            request.Valor,
            request.DataEmissao,
            request.DataVencimento,
            request.Observacoes);

        dbContext.ContasReceber.Add(contaReceber);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ParaResponse(contaReceber);
    }

    public async Task<ContaReceberResponse> AtualizarAsync(Guid id, AtualizarContaReceberRequest request, CancellationToken cancellationToken = default)
    {
        await atualizarValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GarantirReferenciasExistemAsync(request.ClienteId, request.CategoriaId, cancellationToken);

        var contaReceber = await ObterEntidadeAsync(id, cancellationToken);

        contaReceber.Atualizar(request.ClienteId, request.CategoriaId, request.Descricao, request.Valor, request.DataVencimento, request.Observacoes);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ParaResponse(contaReceber);
    }

    public async Task<ContaReceberResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contaReceber = await ObterEntidadeAsync(id, cancellationToken);
        return ParaResponse(contaReceber);
    }

    public async Task<IReadOnlyList<ContaReceberResponse>> ListarAsync(ContaReceberFiltro? filtro = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ContasReceber.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(filtro?.Status)) query = query.Where(c => c.Status.ToString() == filtro.Status);
        if (filtro?.ClienteId is Guid clienteId) query = query.Where(c => c.ClienteId == clienteId);
        if (filtro?.VencimentoInicio is DateTime inicio) query = query.Where(c => c.DataVencimento >= inicio);
        if (filtro?.VencimentoFim is DateTime fim) query = query.Where(c => c.DataVencimento <= fim);
        var contas = await query
            .OrderBy(c => c.DataVencimento)
            .ToListAsync(cancellationToken);

        return contas.Select(ParaResponse).ToList();
    }

    public async Task<ContaReceberResponse> RegistrarRecebimentoAsync(Guid id, RegistrarRecebimentoRequest request, CancellationToken cancellationToken = default)
    {
        await recebimentoValidator.ValidateAndThrowAsync(request, cancellationToken);

        var contaReceber = await ObterEntidadeAsync(id, cancellationToken);
        if (await dbContext.MovimentacoesFinanceiras.AnyAsync(m => m.ContaReceberId == id, cancellationToken))
            throw new RegraNegocioException("Já existe uma movimentação para esta conta a receber.");

        contaReceber.RegistrarRecebimento(request.DataRecebimento);

        var movimentacao = MovimentacaoFinanceira.ParaContaReceber(contaReceber.Id, contaReceber.CategoriaId, contaReceber.Valor, request.DataRecebimento);
        dbContext.MovimentacoesFinanceiras.Add(movimentacao);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ParaResponse(contaReceber);
    }

    public async Task CancelarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contaReceber = await ObterEntidadeAsync(id, cancellationToken);

        contaReceber.Cancelar();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task GarantirReferenciasExistemAsync(Guid clienteId, Guid categoriaId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Clientes.AnyAsync(c => c.Id == clienteId && c.Ativo, cancellationToken))
            throw new RegraNegocioException($"Cliente '{clienteId}' não existe.");

        if (!await dbContext.Categorias.AnyAsync(c => c.Id == categoriaId && c.Ativo && c.Tipo == Domain.Enums.TipoCategoria.Receita, cancellationToken))
            throw new RegraNegocioException($"Categoria '{categoriaId}' não existe.");
    }

    private async Task<ContaReceber> ObterEntidadeAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.ContasReceber.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Conta a receber '{id}' não encontrada.");
    }

    private static ContaReceberResponse ParaResponse(ContaReceber contaReceber)
        => new(
            contaReceber.Id,
            contaReceber.ClienteId,
            contaReceber.CategoriaId,
            contaReceber.Descricao,
            contaReceber.Valor,
            contaReceber.DataEmissao,
            contaReceber.DataVencimento,
            contaReceber.DataRecebimento,
            contaReceber.Status.ToString(),
            contaReceber.Observacoes);
}
