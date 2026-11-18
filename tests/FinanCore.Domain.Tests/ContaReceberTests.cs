using FinanCore.Domain.Entities;
using FinanCore.Domain.Enums;
using FinanCore.Domain.Exceptions;

namespace FinanCore.Domain.Tests;

public class ContaReceberTests
{
    private static ContaReceber CriarContaValida(DateTime? dataVencimento = null, decimal valor = 100m)
    {
        return ContaReceber.Criar(
            clienteId: Guid.NewGuid(),
            categoriaId: Guid.NewGuid(),
            descricao: "Conta de teste",
            valor: valor,
            dataEmissao: new DateTime(2026, 1, 1),
            dataVencimento: dataVencimento ?? new DateTime(2026, 1, 10));
    }

    [Fact]
    public void Criar_ComValorZero_DeveLancarExcecao()
    {
        var act = () => CriarContaValida(valor: 0m);

        Assert.Throws<RegraNegocioException>(act);
    }

    [Fact]
    public void Criar_ComValorNegativo_DeveLancarExcecao()
    {
        var act = () => CriarContaValida(valor: -10m);

        Assert.Throws<RegraNegocioException>(act);
    }

    [Fact]
    public void RegistrarRecebimento_ContaPendente_DeveReceber()
    {
        var conta = CriarContaValida();

        conta.RegistrarRecebimento(new DateTime(2026, 1, 5));

        Assert.Equal(StatusConta.Pago, conta.Status);
        Assert.Equal(new DateTime(2026, 1, 5), conta.DataRecebimento);
    }

    [Fact]
    public void RegistrarRecebimento_ContaVencida_DeveReceber()
    {
        var conta = CriarContaValida(dataVencimento: new DateTime(2026, 1, 5));
        conta.AtualizarStatusVencimento(new DateTime(2026, 1, 10));

        conta.RegistrarRecebimento(new DateTime(2026, 1, 11));

        Assert.Equal(StatusConta.Pago, conta.Status);
    }

    [Fact]
    public void RegistrarRecebimento_ContaJaRecebida_DeveLancarExcecao()
    {
        var conta = CriarContaValida();
        conta.RegistrarRecebimento(new DateTime(2026, 1, 5));

        var act = () => conta.RegistrarRecebimento(new DateTime(2026, 1, 6));

        Assert.Throws<RegraNegocioException>(act);
    }

    [Fact]
    public void RegistrarRecebimento_ContaCancelada_DeveLancarExcecao()
    {
        var conta = CriarContaValida();
        conta.Cancelar();

        var act = () => conta.RegistrarRecebimento(new DateTime(2026, 1, 6));

        Assert.Throws<RegraNegocioException>(act);
    }

    [Fact]
    public void RegistrarRecebimento_SemDataValida_DeveLancarExcecao()
    {
        var conta = CriarContaValida();

        var act = () => conta.RegistrarRecebimento(default);

        Assert.Throws<RegraNegocioException>(act);
    }

    [Fact]
    public void Cancelar_ContaPendente_DeveCancelar()
    {
        var conta = CriarContaValida();

        conta.Cancelar();

        Assert.Equal(StatusConta.Cancelado, conta.Status);
    }

    [Fact]
    public void Cancelar_ContaVencida_DeveCancelar()
    {
        var conta = CriarContaValida(dataVencimento: new DateTime(2026, 1, 5));
        conta.AtualizarStatusVencimento(new DateTime(2026, 1, 10));

        conta.Cancelar();

        Assert.Equal(StatusConta.Cancelado, conta.Status);
    }

    [Fact]
    public void Cancelar_ContaRecebida_DeveLancarExcecao()
    {
        var conta = CriarContaValida();
        conta.RegistrarRecebimento(new DateTime(2026, 1, 5));

        var act = () => conta.Cancelar();

        Assert.Throws<RegraNegocioException>(act);
    }

    [Fact]
    public void Cancelar_ContaJaCancelada_DeveLancarExcecao()
    {
        var conta = CriarContaValida();
        conta.Cancelar();

        var act = () => conta.Cancelar();

        Assert.Throws<RegraNegocioException>(act);
    }

    [Fact]
    public void EstaVencida_ContaPendenteComVencimentoPassado_DeveRetornarTrue()
    {
        var conta = CriarContaValida(dataVencimento: new DateTime(2026, 1, 5));

        var vencida = conta.EstaVencida(new DateTime(2026, 1, 10));

        Assert.True(vencida);
    }

    [Fact]
    public void EstaVencida_ContaComVencimentoHoje_DeveRetornarFalse()
    {
        var conta = CriarContaValida(dataVencimento: new DateTime(2026, 1, 10));

        var vencida = conta.EstaVencida(new DateTime(2026, 1, 10));

        Assert.False(vencida);
    }
}
