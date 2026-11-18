namespace FinanCore.Domain.Entities;

public class ContaReceber : ContaFinanceira
{
    public Guid ClienteId { get; private set; }
    public DateTime? DataRecebimento => DataQuitacao;

    private ContaReceber(
        Guid clienteId,
        Guid categoriaId,
        string descricao,
        decimal valor,
        DateTime dataEmissao,
        DateTime dataVencimento,
        string? observacoes)
        : base(categoriaId, descricao, valor, dataEmissao, dataVencimento, observacoes)
    {
        ClienteId = clienteId;
    }

    public static ContaReceber Criar(
        Guid clienteId,
        Guid categoriaId,
        string descricao,
        decimal valor,
        DateTime dataEmissao,
        DateTime dataVencimento,
        string? observacoes = null)
    {
        return new ContaReceber(clienteId, categoriaId, descricao, valor, dataEmissao, dataVencimento, observacoes);
    }

    public void RegistrarRecebimento(DateTime dataRecebimento) => RegistrarQuitacao(dataRecebimento);

    public void Atualizar(
        Guid clienteId,
        Guid categoriaId,
        string descricao,
        decimal valor,
        DateTime dataVencimento,
        string? observacoes)
    {
        AtualizarDados(categoriaId, descricao, valor, dataVencimento, observacoes);
        ClienteId = clienteId;
    }
}
