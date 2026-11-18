namespace FinanCore.Application.DTOs.ContasPagar;

public record CriarContaPagarRequest(
    Guid FornecedorId,
    Guid CategoriaId,
    string Descricao,
    decimal Valor,
    DateTime DataEmissao,
    DateTime DataVencimento,
    string? Observacoes);

public record AtualizarContaPagarRequest(
    Guid FornecedorId,
    Guid CategoriaId,
    string Descricao,
    decimal Valor,
    DateTime DataVencimento,
    string? Observacoes);

public record RegistrarPagamentoRequest(DateTime DataPagamento);

public record ContaPagarResponse(
    Guid Id,
    Guid FornecedorId,
    Guid CategoriaId,
    string Descricao,
    decimal Valor,
    DateTime DataEmissao,
    DateTime DataVencimento,
    DateTime? DataPagamento,
    string Status,
    string? Observacoes);

public record ContaPagarFiltro(string? Status, Guid? FornecedorId, DateTime? VencimentoInicio, DateTime? VencimentoFim);
