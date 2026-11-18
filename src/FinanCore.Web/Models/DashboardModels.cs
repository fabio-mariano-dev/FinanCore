namespace FinanCore.Web.Models;

public sealed record DashboardResponse(
    decimal SaldoAtual,
    decimal TotalAPagar,
    decimal TotalAReceber,
    decimal EntradasPeriodo,
    decimal SaidasPeriodo,
    decimal ResultadoPeriodo,
    int ContasPagarVencidas,
    int ContasReceberVencidas,
    int QuantidadeContasPagarPeriodo,
    int QuantidadeContasReceberPeriodo,
    DateTime PeriodoInicio,
    DateTime PeriodoFim,
    IReadOnlyList<DashboardChartPoint> Grafico,
    IReadOnlyList<DashboardMovement> MovimentacoesRecentes);

public sealed record DashboardChartPoint(string Mes, decimal Receitas, decimal Despesas);

public sealed record DashboardMovement(
    string Descricao,
    DateTime Data,
    decimal Valor,
    string Tipo,
    string Categoria);
