using FinanCore.Domain.Common;
using FinanCore.Domain.Enums;
using FinanCore.Domain.Exceptions;

namespace FinanCore.Domain.Entities;

public class MovimentacaoFinanceira : BaseEntity
{
    public TipoMovimentacao Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime DataMovimentacao { get; private set; }
    public Guid CategoriaId { get; private set; }
    public Guid? ContaPagarId { get; private set; }
    public Guid? ContaReceberId { get; private set; }

    private MovimentacaoFinanceira(
        TipoMovimentacao tipo,
        decimal valor,
        DateTime dataMovimentacao,
        Guid categoriaId,
        Guid? contaPagarId,
        Guid? contaReceberId)
    {
        if (valor <= 0)
            throw new RegraNegocioException("O valor da movimentação deve ser maior que zero.");

        Tipo = tipo;
        Valor = valor;
        DataMovimentacao = dataMovimentacao;
        CategoriaId = categoriaId;
        ContaPagarId = contaPagarId;
        ContaReceberId = contaReceberId;
    }

    public static MovimentacaoFinanceira ParaContaPagar(Guid contaPagarId, Guid categoriaId, decimal valor, DateTime dataMovimentacao)
        => new(TipoMovimentacao.Saida, valor, dataMovimentacao, categoriaId, contaPagarId, null);

    public static MovimentacaoFinanceira ParaContaReceber(Guid contaReceberId, Guid categoriaId, decimal valor, DateTime dataMovimentacao)
        => new(TipoMovimentacao.Entrada, valor, dataMovimentacao, categoriaId, null, contaReceberId);
}
