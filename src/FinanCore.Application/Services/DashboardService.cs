using FinanCore.Application.Abstractions;
using FinanCore.Application.DTOs.Dashboard;
using FinanCore.Domain.Enums;
using FinanCore.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.Application.Services;

public sealed class DashboardService(IFinanCoreDbContext dbContext, IMovimentacaoFinanceiraService movimentacaoService) : IDashboardService
{
    public async Task<DashboardResponse> ObterAsync(DateTime? periodoInicio = null, DateTime? periodoFim = null, CancellationToken cancellationToken = default)
    {
        var hoje = DateTime.Now;
        var inicioMesAtual = new DateTime(hoje.Year, hoje.Month, 1);

        var inicio = (periodoInicio ?? inicioMesAtual).Date;
        var fim = (periodoFim ?? hoje).Date;

        if (inicio > fim)
            throw new RegraNegocioException("A data inicial do período não pode ser maior que a data final.");

        var totalAPagar = await dbContext.ContasPagar
            .AsNoTracking()
            .Where(conta => conta.Status == StatusConta.Pendente || conta.Status == StatusConta.Vencido)
            .SumAsync(conta => (decimal?)conta.Valor, cancellationToken) ?? 0m;

        var totalAReceber = await dbContext.ContasReceber
            .AsNoTracking()
            .Where(conta => conta.Status == StatusConta.Pendente || conta.Status == StatusConta.Vencido)
            .SumAsync(conta => (decimal?)conta.Valor, cancellationToken) ?? 0m;

        var contasPagarVencidas = await dbContext.ContasPagar
            .AsNoTracking()
            .CountAsync(conta => (conta.Status == StatusConta.Pendente || conta.Status == StatusConta.Vencido)
                && conta.DataVencimento.Date < hoje.Date, cancellationToken);

        var contasReceberVencidas = await dbContext.ContasReceber
            .AsNoTracking()
            .CountAsync(conta => (conta.Status == StatusConta.Pendente || conta.Status == StatusConta.Vencido)
                && conta.DataVencimento.Date < hoje.Date, cancellationToken);

        var quantidadeContasPagarPeriodo = await dbContext.ContasPagar
            .AsNoTracking()
            .CountAsync(conta => conta.DataVencimento.Date >= inicio && conta.DataVencimento.Date <= fim, cancellationToken);

        var quantidadeContasReceberPeriodo = await dbContext.ContasReceber
            .AsNoTracking()
            .CountAsync(conta => conta.DataVencimento.Date >= inicio && conta.DataVencimento.Date <= fim, cancellationToken);

        var fluxoCaixa = await movimentacaoService.ObterFluxoCaixaAsync(inicio, fim, cancellationToken);

        var movimentacoes = await dbContext.MovimentacoesFinanceiras
            .AsNoTracking()
            .OrderByDescending(movimentacao => movimentacao.DataMovimentacao)
            .Select(movimentacao => new
            {
                movimentacao.Tipo,
                movimentacao.Valor,
                movimentacao.DataMovimentacao,
                movimentacao.CategoriaId,
                movimentacao.ContaPagarId,
                movimentacao.ContaReceberId,
                Categoria = dbContext.Categorias
                    .Where(categoria => categoria.Id == movimentacao.CategoriaId)
                    .Select(categoria => categoria.Nome)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var saldoAtual = movimentacoes.Sum(movimentacao =>
            movimentacao.Tipo == TipoMovimentacao.Entrada ? movimentacao.Valor : -movimentacao.Valor);

        var mesAncora = new DateTime(fim.Year, fim.Month, 1);
        var meses = Enumerable.Range(0, 6)
            .Select(indice => mesAncora.AddMonths(indice - 5))
            .ToArray();

        var grafico = meses.Select(mes => new DashboardChartPoint(
            mes.ToString("MMM", new System.Globalization.CultureInfo("pt-BR")),
            movimentacoes
                .Where(movimentacao => movimentacao.Tipo == TipoMovimentacao.Entrada
                    && movimentacao.DataMovimentacao.Year == mes.Year
                    && movimentacao.DataMovimentacao.Month == mes.Month)
                .Sum(movimentacao => movimentacao.Valor),
            movimentacoes
                .Where(movimentacao => movimentacao.Tipo == TipoMovimentacao.Saida
                    && movimentacao.DataMovimentacao.Year == mes.Year
                    && movimentacao.DataMovimentacao.Month == mes.Month)
                .Sum(movimentacao => movimentacao.Valor)))
            .ToList();

        var recentes = movimentacoes
            .Take(4)
            .Select(movimentacao => new DashboardMovement(
                movimentacao.ContaReceberId.HasValue ? "Recebimento de cliente" : "Pagamento fornecedor",
                movimentacao.DataMovimentacao,
                movimentacao.Valor,
                movimentacao.Tipo.ToString(),
                movimentacao.Categoria ?? "Sem categoria"))
            .ToList();

        return new DashboardResponse(
            saldoAtual,
            totalAPagar,
            totalAReceber,
            fluxoCaixa.TotalEntradas,
            fluxoCaixa.TotalSaidas,
            fluxoCaixa.SaldoPeriodo,
            contasPagarVencidas,
            contasReceberVencidas,
            quantidadeContasPagarPeriodo,
            quantidadeContasReceberPeriodo,
            inicio,
            fim,
            grafico,
            recentes);
    }
}
