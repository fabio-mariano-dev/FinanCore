using System.Net.Http.Headers;
using System.Net.Http.Json;
using FinanCore.Web.Models;

namespace FinanCore.Web.Services;

public sealed class ContaPagarService(IHttpClientFactory factory, AuthService auth)
{
    public Task<IReadOnlyList<ContaPagarResponse>?> ListarAsync(
        string? status,
        Guid? fornecedorId,
        DateTime? inicio,
        DateTime? fim,
        CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<ContaPagarResponse>>(
            $"api/contas-pagar?Status={status}&FornecedorId={fornecedorId}&VencimentoInicio={inicio:yyyy-MM-dd}&VencimentoFim={fim:yyyy-MM-dd}",
            ct);

    public Task<ContaPagarResponse?> CriarAsync(
        ContaPagarRequest r,
        CancellationToken ct = default) =>
        SendAsync<ContaPagarResponse>(
            HttpMethod.Post,
            "api/contas-pagar",
            r,
            ct);

    public Task<ContaPagarResponse?> AtualizarAsync(
        Guid id,
        ContaPagarRequest r,
        CancellationToken ct = default) =>
        SendAsync<ContaPagarResponse>(
            HttpMethod.Put,
            $"api/contas-pagar/{id}",
            r,
            ct);

    public Task<ContaPagarResponse?> QuitarAsync(
        Guid id,
        DateTime data,
        CancellationToken ct = default) =>
        SendAsync<ContaPagarResponse>(
            HttpMethod.Patch,
            $"api/contas-pagar/{id}/pagar",
            new { DataPagamento = data },
            ct);

    public Task CancelarAsync(
        Guid id,
        CancellationToken ct = default) =>
        NoContentAsync(
            HttpMethod.Delete,
            $"api/contas-pagar/{id}",
            ct);

    async Task<T?> GetAsync<T>(
        string u,
        CancellationToken ct)
    {
        using var q = new HttpRequestMessage(HttpMethod.Get, u);
        Auth(q);

        using var r = await factory
            .CreateClient("FinanCoreApi")
            .SendAsync(q, ct);

        if (!r.IsSuccessStatusCode)
            throw new HttpRequestException();

        return await r.Content.ReadFromJsonAsync<T>(ct);
    }

    async Task<T?> SendAsync<T>(
        HttpMethod m,
        string u,
        object body,
        CancellationToken ct)
    {
        using var q = new HttpRequestMessage(m, u)
        {
            Content = JsonContent.Create(body)
        };

        Auth(q);

        using var r = await factory
            .CreateClient("FinanCoreApi")
            .SendAsync(q, ct);

        if (!r.IsSuccessStatusCode)
            throw new HttpRequestException();

        return await r.Content.ReadFromJsonAsync<T>(ct);
    }

    async Task NoContentAsync(
        HttpMethod m,
        string u,
        CancellationToken ct)
    {
        using var q = new HttpRequestMessage(m, u);
        Auth(q);

        using var r = await factory
            .CreateClient("FinanCoreApi")
            .SendAsync(q, ct);

        if (!r.IsSuccessStatusCode)
            throw new HttpRequestException();
    }

    void Auth(HttpRequestMessage q)
    {
        if (!string.IsNullOrWhiteSpace(auth.AccessToken))
            q.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", auth.AccessToken);
    }
}
