using System.Net.Http.Headers;
using System.Net.Http.Json;
using FinanCore.Web.Models;

namespace FinanCore.Web.Services;

public sealed class ContaReceberService(IHttpClientFactory factory, AuthService auth)
{
    public Task<IReadOnlyList<ContaReceberResponse>?> ListarAsync(
        string? status,
        Guid? clienteId,
        DateTime? inicio,
        DateTime? fim,
        CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<ContaReceberResponse>>(
            $"api/contas-receber?Status={status}&ClienteId={clienteId}&VencimentoInicio={inicio:yyyy-MM-dd}&VencimentoFim={fim:yyyy-MM-dd}",
            ct);

    public Task<ContaReceberResponse?> CriarAsync(
        ContaReceberRequest r,
        CancellationToken ct = default) =>
        SendAsync<ContaReceberResponse>(
            HttpMethod.Post,
            "api/contas-receber",
            r,
            ct);

    public Task<ContaReceberResponse?> AtualizarAsync(
        Guid id,
        ContaReceberRequest r,
        CancellationToken ct = default) =>
        SendAsync<ContaReceberResponse>(
            HttpMethod.Put,
            $"api/contas-receber/{id}",
            r,
            ct);

    public Task<ContaReceberResponse?> QuitarAsync(
        Guid id,
        DateTime data,
        CancellationToken ct = default) =>
        SendAsync<ContaReceberResponse>(
            HttpMethod.Patch,
            $"api/contas-receber/{id}/receber",
            new { DataRecebimento = data },
            ct);

    public Task CancelarAsync(
        Guid id,
        CancellationToken ct = default) =>
        NoContentAsync(
            HttpMethod.Delete,
            $"api/contas-receber/{id}",
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