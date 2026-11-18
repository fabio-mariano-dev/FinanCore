using FinanCore.Domain.Common;
using FinanCore.Domain.Enums;
using FinanCore.Domain.Exceptions;

namespace FinanCore.Domain.Entities;

public abstract class ContaFinanceira : BaseEntity
{
    public Guid CategoriaId { get; private set; }
    public string Descricao { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime DataEmissao { get; private set; }
    public DateTime DataVencimento { get; private set; }
    public DateTime? DataQuitacao { get; private set; }
    public StatusConta Status { get; private set; }
    public string? Observacoes { get; private set; }

    protected ContaFinanceira(
        Guid categoriaId,
        string descricao,
        decimal valor,
        DateTime dataEmissao,
        DateTime dataVencimento,
        string? observacoes)
    {
        if (valor <= 0)
            throw new RegraNegocioException("O valor da conta deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new RegraNegocioException("A descrição da conta é obrigatória.");

        CategoriaId = categoriaId;
        Descricao = descricao;
        Valor = valor;
        DataEmissao = dataEmissao;
        DataVencimento = dataVencimento;
        Observacoes = observacoes;
        Status = StatusConta.Pendente;
    }

    protected void AtualizarDados(
        Guid categoriaId,
        string descricao,
        decimal valor,
        DateTime dataVencimento,
        string? observacoes)
    {
        if (Status is StatusConta.Pago or StatusConta.Cancelado)
            throw new RegraNegocioException("Não é possível editar uma conta paga/recebida ou cancelada.");

        if (valor <= 0)
            throw new RegraNegocioException("O valor da conta deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new RegraNegocioException("A descrição da conta é obrigatória.");

        CategoriaId = categoriaId;
        Descricao = descricao;
        Valor = valor;
        DataVencimento = dataVencimento;
        Observacoes = observacoes;
    }

    protected void RegistrarQuitacao(DateTime dataQuitacao)
    {
        if (Status == StatusConta.Cancelado)
            throw new RegraNegocioException("Uma conta cancelada não pode ser quitada.");

        if (Status == StatusConta.Pago)
            throw new RegraNegocioException("Esta conta já foi quitada e não pode ser quitada novamente.");

        if (dataQuitacao == default)
            throw new RegraNegocioException("A data de quitação é obrigatória.");

        DataQuitacao = dataQuitacao;
        Status = StatusConta.Pago;
    }

    public void Cancelar()
    {
        if (Status == StatusConta.Pago)
            throw new RegraNegocioException("Uma conta paga/recebida não pode ser cancelada.");

        if (Status == StatusConta.Cancelado)
            throw new RegraNegocioException("Esta conta já está cancelada.");

        Status = StatusConta.Cancelado;
    }

    public void AtualizarStatusVencimento(DateTime hoje)
    {
        if (Status == StatusConta.Pendente && DataVencimento.Date < hoje.Date)
            Status = StatusConta.Vencido;
    }

    public bool EstaVencida(DateTime hoje)
    {
        return Status is StatusConta.Pendente or StatusConta.Vencido
            && DataVencimento.Date < hoje.Date;
    }
}
