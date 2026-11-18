using System.Net.Http.Headers;
using System.Net.Http.Json;
using FinanCore.Web.Models;

namespace FinanCore.Web.Services;

public sealed class ClienteService(IHttpClientFactory httpClientFactory, AuthService authService)
{
    public async Task<IReadOnlyList<ClienteResponse>> ListarAsync(CancellationToken cancellationToken = default)
        => await SendAsync<IReadOnlyList<ClienteResponse>>(HttpMethod.Get, "api/clientes", null, cancellationToken) ?? [];

    public Task<ClienteResponse?> CriarAsync(ClienteRequest request, CancellationToken cancellationToken = default)
        => SendAsync<ClienteResponse>(HttpMethod.Post, "api/clientes", request, cancellationToken);

    public Task<ClienteResponse?> AtualizarAsync(Guid id, ClienteRequest request, CancellationToken cancellationToken = default)
        => SendAsync<ClienteResponse>(HttpMethod.Put, $"api/clientes/{id}", request, cancellationToken);

    public Task InativarAsync(Guid id, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Delete, $"api/clientes/{id}", cancellationToken);

    public Task AtivarAsync(Guid id, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Patch, $"api/clientes/{id}/ativar", cancellationToken);

    private async Task<T?> SendAsync<T>(HttpMethod method, string uri, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, uri);
        AddAuthorization(request);
        if (body is not null) request.Content = JsonContent.Create(body);
        using var response = await httpClientFactory.CreateClient("FinanCoreApi").SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Clientes API retornou {(int)response.StatusCode}.");
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    private async Task SendNoContentAsync(HttpMethod method, string uri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, uri);
        AddAuthorization(request);
        using var response = await httpClientFactory.CreateClient("FinanCoreApi").SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Clientes API retornou {(int)response.StatusCode}.");
    }

    private void AddAuthorization(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(authService.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authService.AccessToken);
    }
}
