namespace FinanCore.Application.DTOs.ContasReceber;

public record CriarContaReceberRequest(
    Guid ClienteId,
    Guid CategoriaId,
    string Descricao,
    decimal Valor,
    DateTime DataEmissao,
    DateTime DataVencimento,
    string? Observacoes);

public record AtualizarContaReceberRequest(
    Guid ClienteId,
    Guid CategoriaId,
    string Descricao,
    decimal Valor,
    DateTime DataVencimento,
    string? Observacoes);

public record RegistrarRecebimentoRequest(DateTime DataRecebimento);

public record ContaReceberResponse(
    Guid Id,
    Guid ClienteId,
    Guid CategoriaId,
    string Descricao,
    decimal Valor,
    DateTime DataEmissao,
    DateTime DataVencimento,
    DateTime? DataRecebimento,
    string Status,
    string? Observacoes);

public record ContaReceberFiltro(string? Status, Guid? ClienteId, DateTime? VencimentoInicio, DateTime? VencimentoFim);
