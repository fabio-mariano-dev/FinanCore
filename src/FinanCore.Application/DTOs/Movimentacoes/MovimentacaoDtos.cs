namespace FinanCore.Application.DTOs.Movimentacoes;

public record MovimentacaoFiltro(
    DateTime? PeriodoInicio,
    DateTime? PeriodoFim,
    string? Tipo,
    Guid? CategoriaId);

public record MovimentacaoResponse(
    Guid Id,
    string Tipo,
    decimal Valor,
    DateTime DataMovimentacao,
    Guid CategoriaId,
    string CategoriaNome,
    string Origem,
    string Descricao);

public record FluxoCaixaResponse(
    decimal SaldoInicial,
    decimal TotalEntradas,
    decimal TotalSaidas,
    decimal SaldoPeriodo,
    decimal SaldoFinal,
    IReadOnlyList<MovimentacaoResponse> Movimentacoes);
