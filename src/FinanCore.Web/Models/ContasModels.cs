namespace FinanCore.Web.Models;

public enum StatusContaWeb { Pendente = 1, Pago = 2, Vencido = 3, Cancelado = 4 }
public sealed record ContaReceberResponse(Guid Id, Guid ClienteId, Guid CategoriaId, string Descricao, decimal Valor, DateTime DataEmissao, DateTime DataVencimento, DateTime? DataRecebimento, string Status, string? Observacoes);
public sealed record ContaPagarResponse(Guid Id, Guid FornecedorId, Guid CategoriaId, string Descricao, decimal Valor, DateTime DataEmissao, DateTime DataVencimento, DateTime? DataPagamento, string Status, string? Observacoes);
public sealed record ContaReceberRequest(Guid ClienteId, Guid CategoriaId, string Descricao, decimal Valor, DateTime DataEmissao, DateTime DataVencimento, string? Observacoes);
public sealed record ContaPagarRequest(Guid FornecedorId, Guid CategoriaId, string Descricao, decimal Valor, DateTime DataEmissao, DateTime DataVencimento, string? Observacoes);
