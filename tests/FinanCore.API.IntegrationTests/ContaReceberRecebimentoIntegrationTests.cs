using System.Net;
using System.Net.Http.Json;
using FinanCore.Application.DTOs.ContasReceber;
using FinanCore.Domain.Entities;
using FinanCore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.API.IntegrationTests;

public class ContaReceberRecebimentoIntegrationTests(FinanCoreApiFactory factory) : IClassFixture<FinanCoreApiFactory>
{
    private async Task<Guid> SeedContaPendenteAsync()
    {
        using var scope = factory.CreateDbScope();
        var db = scope.GetDbContext();

        var cliente = new Cliente($"Cliente {Guid.NewGuid()}");
        var categoria = new Categoria($"Categoria {Guid.NewGuid()}", TipoCategoria.Receita);
        var conta = ContaReceber.Criar(cliente.Id, categoria.Id, "Conta de teste", 250m, new DateTime(2026, 1, 1), new DateTime(2026, 2, 1));

        db.Clientes.Add(cliente);
        db.Categorias.Add(categoria);
        db.ContasReceber.Add(conta);
        await db.SaveChangesAsync();

        return conta.Id;
    }

    [Fact]
    public async Task RegistrarRecebimento_ContaInexistente_DeveRetornar404()
    {
        var client = factory.CreateAuthenticatedClient();

        var response = await client.PatchAsJsonAsync($"/api/contas-receber/{Guid.NewGuid()}/receber", new RegistrarRecebimentoRequest(new DateTime(2026, 1, 15)));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarRecebimento_DuasRequisicoesSimultaneas_ApenasUmaDeveConcluir()
    {
        var contaId = await SeedContaPendenteAsync();
        var client = factory.CreateAuthenticatedClient();

        var request1 = client.PatchAsJsonAsync($"/api/contas-receber/{contaId}/receber", new RegistrarRecebimentoRequest(new DateTime(2026, 1, 15)));
        var request2 = client.PatchAsJsonAsync($"/api/contas-receber/{contaId}/receber", new RegistrarRecebimentoRequest(new DateTime(2026, 1, 16)));
        var respostas = await Task.WhenAll(request1, request2);

        Assert.Single(respostas, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Single(respostas, r => r.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest);

        using var scope = factory.CreateDbScope();
        var db = scope.GetDbContext();

        var movimentacoes = await db.MovimentacoesFinanceiras.Where(m => m.ContaReceberId == contaId).ToListAsync();
        var movimentacao = Assert.Single(movimentacoes);
        Assert.Equal(TipoMovimentacao.Entrada, movimentacao.Tipo);

        var contaFinal = await db.ContasReceber.AsNoTracking().SingleAsync(c => c.Id == contaId);
        Assert.Equal(StatusConta.Pago, contaFinal.Status);
    }

    [Fact]
    public async Task RegistrarRecebimento_FalhaAoPersistirMovimentacao_DeveReverterStatusENaoDeixarMovimentacaoParcial()
    {
        var contaId = await SeedContaPendenteAsync();

        Guid categoriaId;
        using (var seedScope = factory.CreateDbScope())
        {
            var seedDb = seedScope.GetDbContext();
            var conta = await seedDb.ContasReceber.AsNoTracking().SingleAsync(c => c.Id == contaId);
            categoriaId = conta.CategoriaId;

            seedDb.MovimentacoesFinanceiras.Add(MovimentacaoFinanceira.ParaContaReceber(contaId, categoriaId, conta.Valor, new DateTime(2026, 1, 10)));
            await seedDb.SaveChangesAsync();
        }

        var client = factory.CreateAuthenticatedClient();
        var response = await client.PatchAsJsonAsync($"/api/contas-receber/{contaId}/receber", new RegistrarRecebimentoRequest(new DateTime(2026, 1, 20)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = factory.CreateDbScope();
        var db = scope.GetDbContext();

        var contaFinal = await db.ContasReceber.AsNoTracking().SingleAsync(c => c.Id == contaId);
        Assert.Equal(StatusConta.Pendente, contaFinal.Status);
        Assert.Null(contaFinal.DataQuitacao);

        var movimentacoes = await db.MovimentacoesFinanceiras.Where(m => m.ContaReceberId == contaId).ToListAsync();
        Assert.Single(movimentacoes);
    }
}
