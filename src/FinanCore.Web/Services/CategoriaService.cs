using System.Net.Http.Headers;
using System.Net.Http.Json;
using FinanCore.Web.Models;

namespace FinanCore.Web.Services;

public sealed class CategoriaService(IHttpClientFactory httpClientFactory, AuthService authService)
{
    public async Task<IReadOnlyList<CategoriaResponse>> ListarAsync(CancellationToken cancellationToken = default)
        => await SendAsync<IReadOnlyList<CategoriaResponse>>(HttpMethod.Get, "api/categorias", null, cancellationToken)
            ?? [];

    public Task<CategoriaResponse?> CriarAsync(CategoriaRequest request, CancellationToken cancellationToken = default)
        => SendAsync<CategoriaResponse>(HttpMethod.Post, "api/categorias", request, cancellationToken);

    public Task<CategoriaResponse?> AtualizarAsync(Guid id, CategoriaRequest request, CancellationToken cancellationToken = default)
        => SendAsync<CategoriaResponse>(HttpMethod.Put, $"api/categorias/{id}", request, cancellationToken);

    public Task InativarAsync(Guid id, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Delete, $"api/categorias/{id}", cancellationToken);

    public Task AtivarAsync(Guid id, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Patch, $"api/categorias/{id}/ativar", cancellationToken);

    private async Task<T?> SendAsync<T>(HttpMethod method, string uri, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, uri);
        AddAuthorization(request);
        if (body is not null)
            request.Content = JsonContent.Create(body);

        using var response = await httpClientFactory.CreateClient("FinanCoreApi").SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Categorias API retornou {(int)response.StatusCode}.");

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    private async Task SendNoContentAsync(HttpMethod method, string uri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, uri);
        AddAuthorization(request);
        using var response = await httpClientFactory.CreateClient("FinanCoreApi").SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Categorias API retornou {(int)response.StatusCode}.");
    }

    private void AddAuthorization(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(authService.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authService.AccessToken);
    }
}
