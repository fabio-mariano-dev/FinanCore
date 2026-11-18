using FinanCore.Application.DTOs.ContasPagar;
using FinanCore.Application.Services;
using FinanCore.Application.Validators.ContasPagar;
using FinanCore.Domain.Entities;
using FinanCore.Domain.Enums;
using FinanCore.Domain.Exceptions;
using FinanCore.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.Application.Tests;

public class ContaPagarServiceTests
{
    private static FinanCoreDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanCoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FinanCoreDbContext(options);
    }

    private static ContaPagarService CriarService(FinanCoreDbContext dbContext)
        => new(
            dbContext,
            new CriarContaPagarRequestValidator(),
            new AtualizarContaPagarRequestValidator(),
            new RegistrarPagamentoRequestValidator());

    private static async Task<ContaPagar> SeedContaPendenteAsync(FinanCoreDbContext dbContext, DateTime dataVencimento)
    {
        var fornecedor = new Fornecedor("Fornecedor Teste");
        var categoria = new Categoria("Categoria Teste", TipoCategoria.Despesa);
        var contaPagar = ContaPagar.Criar(fornecedor.Id, categoria.Id, "Conta de teste", 100m, new DateTime(2026, 1, 1), dataVencimento);

        dbContext.Fornecedores.Add(fornecedor);
        dbContext.Categorias.Add(categoria);
        dbContext.ContasPagar.Add(contaPagar);
        await dbContext.SaveChangesAsync();

        return contaPagar;
    }

    [Fact]
    public async Task RegistrarPagamentoAsync_ContaPendente_DeveAlterarStatusECriarMovimentacaoDeSaida()
    {
        await using var dbContext = CriarDbContext();
        var conta = await SeedContaPendenteAsync(dbContext, new DateTime(2026, 2, 1));
        var service = CriarService(dbContext);

        var response = await service.RegistrarPagamentoAsync(conta.Id, new RegistrarPagamentoRequest(new DateTime(2026, 1, 15)));

        Assert.Equal("Pago", response.Status);
        Assert.Equal(new DateTime(2026, 1, 15), response.DataPagamento);

        var movimentacoes = await dbContext.MovimentacoesFinanceiras.Where(m => m.ContaPagarId == conta.Id).ToListAsync();
        var movimentacao = Assert.Single(movimentacoes);
        Assert.Equal(TipoMovimentacao.Saida, movimentacao.Tipo);
        Assert.Equal(conta.Valor, movimentacao.Valor);
        Assert.Equal(conta.CategoriaId, movimentacao.CategoriaId);
        Assert.Null(movimentacao.ContaReceberId);
    }

    [Fact]
    public async Task RegistrarPagamentoAsync_ContaVencida_DevePagar()
    {
        await using var dbContext = CriarDbContext();
        var conta = await SeedContaPendenteAsync(dbContext, new DateTime(2026, 1, 5));
        conta.AtualizarStatusVencimento(new DateTime(2026, 1, 10));
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var response = await service.RegistrarPagamentoAsync(conta.Id, new RegistrarPagamentoRequest(new DateTime(2026, 1, 11)));

        Assert.Equal("Pago", response.Status);
    }

    [Fact]
    public async Task RegistrarPagamentoAsync_ContaJaPaga_DeveLancarRegraNegocioException()
    {
        await using var dbContext = CriarDbContext();
        var conta = await SeedContaPendenteAsync(dbContext, new DateTime(2026, 2, 1));
        var service = CriarService(dbContext);
        await service.RegistrarPagamentoAsync(conta.Id, new RegistrarPagamentoRequest(new DateTime(2026, 1, 15)));

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => service.RegistrarPagamentoAsync(conta.Id, new RegistrarPagamentoRequest(new DateTime(2026, 1, 16))));

        var movimentacoes = await dbContext.MovimentacoesFinanceiras.Where(m => m.ContaPagarId == conta.Id).ToListAsync();
        Assert.Single(movimentacoes);
    }

    [Fact]
    public async Task RegistrarPagamentoAsync_ContaCancelada_DeveLancarRegraNegocioException()
    {
        await using var dbContext = CriarDbContext();
        var conta = await SeedContaPendenteAsync(dbContext, new DateTime(2026, 2, 1));
        conta.Cancelar();
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => service.RegistrarPagamentoAsync(conta.Id, new RegistrarPagamentoRequest(new DateTime(2026, 1, 16))));

        Assert.False(await dbContext.MovimentacoesFinanceiras.AnyAsync(m => m.ContaPagarId == conta.Id));
    }

    [Fact]
    public async Task RegistrarPagamentoAsync_ContaInexistente_DeveLancarKeyNotFoundException()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.RegistrarPagamentoAsync(Guid.NewGuid(), new RegistrarPagamentoRequest(new DateTime(2026, 1, 16))));
    }

    [Fact]
    public async Task RegistrarPagamentoAsync_RequestInvalido_NaoDeveAlterarContaNemCriarMovimentacao()
    {
        await using var dbContext = CriarDbContext();
        var conta = await SeedContaPendenteAsync(dbContext, new DateTime(2026, 2, 1));
        var service = CriarService(dbContext);

        await Assert.ThrowsAsync<ValidationException>(
            () => service.RegistrarPagamentoAsync(conta.Id, new RegistrarPagamentoRequest(default)));

        var contaNoBanco = await dbContext.ContasPagar.AsNoTracking().SingleAsync(c => c.Id == conta.Id);
        Assert.Equal(StatusConta.Pendente, contaNoBanco.Status);
        Assert.Null(contaNoBanco.DataQuitacao);
        Assert.False(await dbContext.MovimentacoesFinanceiras.AnyAsync(m => m.ContaPagarId == conta.Id));
    }
}
