using System.Net;
using System.Net.Http.Json;
using FinanCore.Application.DTOs.ContasPagar;
using FinanCore.Domain.Entities;
using FinanCore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.API.IntegrationTests;

public class ContaPagarPagamentoIntegrationTests(FinanCoreApiFactory factory) : IClassFixture<FinanCoreApiFactory>
{
    private async Task<Guid> SeedContaPendenteAsync()
    {
        using var scope = factory.CreateDbScope();
        var db = scope.GetDbContext();

        var fornecedor = new Fornecedor($"Fornecedor {Guid.NewGuid()}");
        var categoria = new Categoria($"Categoria {Guid.NewGuid()}", TipoCategoria.Despesa);
        var conta = ContaPagar.Criar(fornecedor.Id, categoria.Id, "Conta de teste", 150m, new DateTime(2026, 1, 1), new DateTime(2026, 2, 1));

        db.Fornecedores.Add(fornecedor);
        db.Categorias.Add(categoria);
        db.ContasPagar.Add(conta);
        await db.SaveChangesAsync();

        return conta.Id;
    }

    [Fact]
    public async Task RegistrarPagamento_ContaInexistente_DeveRetornar404()
    {
        var client = factory.CreateAuthenticatedClient();

        var response = await client.PatchAsJsonAsync($"/api/contas-pagar/{Guid.NewGuid()}/pagar", new RegistrarPagamentoRequest(new DateTime(2026, 1, 15)));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarPagamento_DuasRequisicoesSimultaneas_ApenasUmaDeveConcluir()
    {
        var contaId = await SeedContaPendenteAsync();
        var client = factory.CreateAuthenticatedClient();

        var request1 = client.PatchAsJsonAsync($"/api/contas-pagar/{contaId}/pagar", new RegistrarPagamentoRequest(new DateTime(2026, 1, 15)));
        var request2 = client.PatchAsJsonAsync($"/api/contas-pagar/{contaId}/pagar", new RegistrarPagamentoRequest(new DateTime(2026, 1, 16)));
        var respostas = await Task.WhenAll(request1, request2);

        Assert.Single(respostas, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Single(respostas, r => r.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest);

        using var scope = factory.CreateDbScope();
        var db = scope.GetDbContext();

        var movimentacoes = await db.MovimentacoesFinanceiras.Where(m => m.ContaPagarId == contaId).ToListAsync();
        var movimentacao = Assert.Single(movimentacoes);
        Assert.Equal(TipoMovimentacao.Saida, movimentacao.Tipo);

        var contaFinal = await db.ContasPagar.AsNoTracking().SingleAsync(c => c.Id == contaId);
        Assert.Equal(StatusConta.Pago, contaFinal.Status);
    }

    [Fact]
    public async Task RegistrarPagamento_FalhaAoPersistirMovimentacao_DeveReverterStatusENaoDeixarMovimentacaoParcial()
    {
        var contaId = await SeedContaPendenteAsync();

        Guid categoriaId;
        using (var seedScope = factory.CreateDbScope())
        {
            var seedDb = seedScope.GetDbContext();
            var conta = await seedDb.ContasPagar.AsNoTracking().SingleAsync(c => c.Id == contaId);
            categoriaId = conta.CategoriaId;

            seedDb.MovimentacoesFinanceiras.Add(MovimentacaoFinanceira.ParaContaPagar(contaId, categoriaId, conta.Valor, new DateTime(2026, 1, 10)));
            await seedDb.SaveChangesAsync();
        }

        var client = factory.CreateAuthenticatedClient();
        var response = await client.PatchAsJsonAsync($"/api/contas-pagar/{contaId}/pagar", new RegistrarPagamentoRequest(new DateTime(2026, 1, 20)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = factory.CreateDbScope();
        var db = scope.GetDbContext();

        var contaFinal = await db.ContasPagar.AsNoTracking().SingleAsync(c => c.Id == contaId);
        Assert.Equal(StatusConta.Pendente, contaFinal.Status);
        Assert.Null(contaFinal.DataQuitacao);

        var movimentacoes = await db.MovimentacoesFinanceiras.Where(m => m.ContaPagarId == contaId).ToListAsync();
        Assert.Single(movimentacoes);
    }
}
