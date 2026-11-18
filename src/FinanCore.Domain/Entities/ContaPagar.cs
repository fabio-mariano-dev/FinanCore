namespace FinanCore.Domain.Entities;

public class ContaPagar : ContaFinanceira
{
    public Guid FornecedorId { get; private set; }
    public DateTime? DataPagamento => DataQuitacao;

    private ContaPagar(
        Guid fornecedorId,
        Guid categoriaId,
        string descricao,
        decimal valor,
        DateTime dataEmissao,
        DateTime dataVencimento,
        string? observacoes)
        : base(categoriaId, descricao, valor, dataEmissao, dataVencimento, observacoes)
    {
        FornecedorId = fornecedorId;
    }

    public static ContaPagar Criar(
        Guid fornecedorId,
        Guid categoriaId,
        string descricao,
        decimal valor,
        DateTime dataEmissao,
        DateTime dataVencimento,
        string? observacoes = null)
    {
        return new ContaPagar(fornecedorId, categoriaId, descricao, valor, dataEmissao, dataVencimento, observacoes);
    }

    public void RegistrarPagamento(DateTime dataPagamento) => RegistrarQuitacao(dataPagamento);

    public void Atualizar(
        Guid fornecedorId,
        Guid categoriaId,
        string descricao,
        decimal valor,
        DateTime dataVencimento,
        string? observacoes)
    {
        AtualizarDados(categoriaId, descricao, valor, dataVencimento, observacoes);
        FornecedorId = fornecedorId;
    }
}
