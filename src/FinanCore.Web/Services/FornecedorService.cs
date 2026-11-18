using System.Net.Http.Headers;
using System.Net.Http.Json;
using FinanCore.Web.Models;

namespace FinanCore.Web.Services;

public sealed class FornecedorService(
    IHttpClientFactory httpClientFactory,
    AuthService authService)
{
    public async Task<IReadOnlyList<FornecedorResponse>> ListarAsync(
        CancellationToken cancellationToken = default) =>
        await SendAsync<IReadOnlyList<FornecedorResponse>>(
            HttpMethod.Get,
            "api/fornecedores",
            null,
            cancellationToken) ?? [];

    public Task<FornecedorResponse?> CriarAsync(
        FornecedorRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<FornecedorResponse>(
            HttpMethod.Post,
            "api/fornecedores",
            request,
            cancellationToken);

    public Task<FornecedorResponse?> AtualizarAsync(
        Guid id,
        FornecedorRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<FornecedorResponse>(
            HttpMethod.Put,
            $"api/fornecedores/{id}",
            request,
            cancellationToken);

    public Task InativarAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        SendNoContentAsync(
            HttpMethod.Delete,
            $"api/fornecedores/{id}",
            cancellationToken);

    public Task AtivarAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        SendNoContentAsync(
            HttpMethod.Patch,
            $"api/fornecedores/{id}/ativar",
            cancellationToken);

    private async Task<T?> SendAsync<T>(
        HttpMethod method,
        string uri,
        object? body,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, uri);
        AddAuthorization(request);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        using var response = await httpClientFactory
            .CreateClient("FinanCoreApi")
            .SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Fornecedores API retornou {(int)response.StatusCode}.");

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    private async Task SendNoContentAsync(
        HttpMethod method,
        string uri,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, uri);
        AddAuthorization(request);

        using var response = await httpClientFactory
            .CreateClient("FinanCoreApi")
            .SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Fornecedores API retornou {(int)response.StatusCode}.");
    }

    private void AddAuthorization(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(authService.AccessToken))
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    authService.AccessToken);
    }
}