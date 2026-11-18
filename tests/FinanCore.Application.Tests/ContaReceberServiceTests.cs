using FinanCore.Application.DTOs.ContasReceber;
using FinanCore.Application.Services;
using FinanCore.Application.Validators.ContasReceber;
using FinanCore.Domain.Entities;
using FinanCore.Domain.Enums;
using FinanCore.Domain.Exceptions;
using FinanCore.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.Application.Tests;

public class ContaReceberServiceTests
{
    private static FinanCoreDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanCoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FinanCoreDbContext(options);
    }

    private static ContaReceberService CriarService(FinanCoreDbContext dbContext)
        => new(
            dbContext,
            new CriarContaReceberRequestValidator(),
            new AtualizarContaReceberRequestValidator(),
            new RegistrarRecebimentoRequestValidator());

    private static async Task<ContaReceber> SeedContaPendenteAsync(FinanCoreDbContext dbContext, DateTime dataVencimento)
    {
        var cliente = new Cliente("Cliente Teste");
        var categoria = new Categoria("Categoria Teste", TipoCategoria.Receita);
        var contaReceber = ContaReceber.Criar(cliente.Id, categoria.Id, "Conta de teste", 200m, new DateTime(2026, 1, 1), dataVencimento);

        dbContext.Clientes.Add(cliente);
        dbContext.Categorias.Add(categoria);
        dbContext.ContasReceber.Add(contaReceber);
        await dbContext.SaveChangesAsync();

        return contaReceber;
    }

    [Fact]
    public async Task RegistrarRecebimentoAsync_ContaPendente_DeveAlterarStatusECriarMovimentacaoDeEntrada()
    {
        await using var dbContext = CriarDbContext();
        var conta = await SeedContaPendenteAsync(dbContext, new DateTime(2026, 2, 1));
        var service = CriarService(dbContext);

        var response = await service.RegistrarRecebimentoAsync(conta.Id, new RegistrarRecebimentoRequest(new DateTime(2026, 1, 15)));

        Assert.Equal("Pago", response.Status);
        Assert.Equal(new DateTime(2026, 1, 15), response.DataRecebimento);

        var movimentacoes = await dbContext.MovimentacoesFinanceiras.Where(m => m.ContaReceberId == conta.Id).ToListAsync();
        var movimentacao = Assert.Single(movimentacoes);
        Assert.Equal(TipoMovimentacao.Entrada, movimentacao.Tipo);
        Assert.Equal(conta.Valor, movimentacao.Valor);
        Assert.Equal(conta.CategoriaId, movimentacao.CategoriaId);
        Assert.Null(movimentacao.ContaPagarId);
    }

    [Fact]
    public async Task RegistrarRecebimentoAsync_ContaVencida_DeveReceber()
    {
        await using var dbContext = CriarDbContext();
        var conta = await SeedContaPendenteAsync(dbContext, new DateTime(2026, 1, 5));
        conta.AtualizarStatusVencimento(new DateTime(2026, 1, 10));
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var response = await service.RegistrarRecebimentoAsync(conta.Id, new RegistrarRecebimentoRequest(new DateTime(2026, 1, 11)));

        Assert.Equal("Pago", response.Status);
    }

    [Fact]
    public async Task RegistrarRecebimentoAsync_ContaJaRecebida_DeveLancarRegraNegocioException()
    {
        await using var dbContext = CriarDbContext();
        var conta = await SeedContaPendenteAsync(dbContext, new DateTime(2026, 2, 1));
        var service = CriarService(dbContext);
        await service.RegistrarRecebimentoAsync(conta.Id, new RegistrarRecebimentoRequest(new DateTime(2026, 1, 15)));

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => service.RegistrarRecebimentoAsync(conta.Id, new RegistrarRecebimentoRequest(new DateTime(2026, 1, 16))));

        var movimentacoes = await dbContext.MovimentacoesFinanceiras.Where(m => m.ContaReceberId == conta.Id).ToListAsync();
        Assert.Single(movimentacoes);
    }

    [Fact]
    public async Task RegistrarRecebimentoAsync_ContaCancelada_DeveLancarRegraNegocioException()
    {
        await using var dbContext = CriarDbContext();
        var conta = await SeedContaPendenteAsync(dbContext, new DateTime(2026, 2, 1));
        conta.Cancelar();
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => service.RegistrarRecebimentoAsync(conta.Id, new RegistrarRecebimentoRequest(new DateTime(2026, 1, 16))));

        Assert.False(await dbContext.MovimentacoesFinanceiras.AnyAsync(m => m.ContaReceberId == conta.Id));
    }

    [Fact]
    public async Task RegistrarRecebimentoAsync_ContaInexistente_DeveLancarKeyNotFoundException()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.RegistrarRecebimentoAsync(Guid.NewGuid(), new RegistrarRecebimentoRequest(new DateTime(2026, 1, 16))));
    }

    [Fact]
    public async Task RegistrarRecebimentoAsync_RequestInvalido_NaoDeveAlterarContaNemCriarMovimentacao()
    {
        await using var dbContext = CriarDbContext();
        var conta = await SeedContaPendenteAsync(dbContext, new DateTime(2026, 2, 1));
        var service = CriarService(dbContext);

        await Assert.ThrowsAsync<ValidationException>(
            () => service.RegistrarRecebimentoAsync(conta.Id, new RegistrarRecebimentoRequest(default)));

        var contaNoBanco = await dbContext.ContasReceber.AsNoTracking().SingleAsync(c => c.Id == conta.Id);
        Assert.Equal(StatusConta.Pendente, contaNoBanco.Status);
        Assert.Null(contaNoBanco.DataQuitacao);
        Assert.False(await dbContext.MovimentacoesFinanceiras.AnyAsync(m => m.ContaReceberId == conta.Id));
    }
}
