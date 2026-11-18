using FinanCore.Application.Abstractions;
using FinanCore.Application.DTOs.Movimentacoes;
using FinanCore.Domain.Enums;
using FinanCore.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.Application.Services;

public class MovimentacaoFinanceiraService(IFinanCoreDbContext dbContext) : IMovimentacaoFinanceiraService
{
    public async Task<IReadOnlyList<MovimentacaoResponse>> ListarAsync(MovimentacaoFiltro? filtro = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.MovimentacoesFinanceiras.AsNoTracking().AsQueryable();

        if (filtro?.PeriodoInicio is DateTime inicio) query = query.Where(m => m.DataMovimentacao >= inicio.Date);
        if (filtro?.PeriodoFim is DateTime fim) query = query.Where(m => m.DataMovimentacao <= fim.Date.AddDays(1).AddTicks(-1));
        if (filtro?.CategoriaId is Guid categoriaId) query = query.Where(m => m.CategoriaId == categoriaId);
        if (!string.IsNullOrWhiteSpace(filtro?.Tipo) && Enum.TryParse<TipoMovimentacao>(filtro.Tipo, true, out var tipo))
            query = query.Where(m => m.Tipo == tipo);

        var movimentacoes = await query
            .OrderByDescending(m => m.DataMovimentacao)
            .ToListAsync(cancellationToken);

        if (movimentacoes.Count == 0)
            return [];

        var categorias = await dbContext.Categorias.AsNoTracking()
            .Where(c => movimentacoes.Select(m => m.CategoriaId).Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Nome, cancellationToken);

        var contasPagarIds = movimentacoes.Where(m => m.ContaPagarId.HasValue).Select(m => m.ContaPagarId!.Value).ToList();
        var contasReceberIds = movimentacoes.Where(m => m.ContaReceberId.HasValue).Select(m => m.ContaReceberId!.Value).ToList();

        var descricoesContasPagar = await dbContext.ContasPagar.AsNoTracking()
            .Where(c => contasPagarIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Descricao, cancellationToken);

        var descricoesContasReceber = await dbContext.ContasReceber.AsNoTracking()
            .Where(c => contasReceberIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Descricao, cancellationToken);

        return movimentacoes.Select(m => ParaResponse(m, categorias, descricoesContasPagar, descricoesContasReceber)).ToList();
    }

    public async Task<FluxoCaixaResponse> ObterFluxoCaixaAsync(DateTime periodoInicio, DateTime periodoFim, CancellationToken cancellationToken = default)
    {
        var inicio = periodoInicio.Date;
        var fim = periodoFim.Date;

        if (inicio > fim)
            throw new RegraNegocioException("A data inicial do período não pode ser maior que a data final.");

        var movimentacoesAnteriores = await dbContext.MovimentacoesFinanceiras.AsNoTracking()
            .Where(m => m.DataMovimentacao < inicio)
            .Select(m => new { m.Tipo, m.Valor })
            .ToListAsync(cancellationToken);

        var saldoInicial = movimentacoesAnteriores.Sum(m => m.Tipo == TipoMovimentacao.Entrada ? m.Valor : -m.Valor);

        var movimentacoes = await ListarAsync(new MovimentacaoFiltro(inicio, fim, null, null), cancellationToken);

        var totalEntradas = movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Entrada.ToString()).Sum(m => m.Valor);
        var totalSaidas = movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Saida.ToString()).Sum(m => m.Valor);
        var saldoPeriodo = totalEntradas - totalSaidas;

        return new FluxoCaixaResponse(saldoInicial, totalEntradas, totalSaidas, saldoPeriodo, saldoInicial + saldoPeriodo, movimentacoes);
    }

    private static MovimentacaoResponse ParaResponse(
        Domain.Entities.MovimentacaoFinanceira movimentacao,
        IReadOnlyDictionary<Guid, string> categorias,
        IReadOnlyDictionary<Guid, string> descricoesContasPagar,
        IReadOnlyDictionary<Guid, string> descricoesContasReceber)
    {
        var origem = movimentacao.ContaPagarId.HasValue ? "ContaPagar" : "ContaReceber";
        var descricao = movimentacao.ContaPagarId is Guid contaPagarId
            ? descricoesContasPagar.GetValueOrDefault(contaPagarId, "-")
            : descricoesContasReceber.GetValueOrDefault(movimentacao.ContaReceberId!.Value, "-");

        return new MovimentacaoResponse(
            movimentacao.Id,
            movimentacao.Tipo.ToString(),
            movimentacao.Valor,
            movimentacao.DataMovimentacao,
            movimentacao.CategoriaId,
            categorias.GetValueOrDefault(movimentacao.CategoriaId, "-"),
            origem,
            descricao);
    }
}
