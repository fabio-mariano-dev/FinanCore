namespace FinanCore.Web.Models;

public sealed record MovimentacaoResponse(
    Guid Id,
    string Tipo,
    decimal Valor,
    DateTime DataMovimentacao,
    Guid CategoriaId,
    string CategoriaNome,
    string Origem,
    string Descricao);

public sealed record FluxoCaixaResponse(
    decimal SaldoInicial,
    decimal TotalEntradas,
    decimal TotalSaidas,
    decimal SaldoPeriodo,
    decimal SaldoFinal,
    IReadOnlyList<MovimentacaoResponse> Movimentacoes);
