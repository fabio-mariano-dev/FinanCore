using System.Net.Http.Headers;
using System.Net.Http.Json;
using FinanCore.Web.Models;

namespace FinanCore.Web.Services;

public sealed class MovimentacaoService(IHttpClientFactory httpClientFactory, AuthService authService)
{
    public async Task<IReadOnlyList<MovimentacaoResponse>> ListarAsync(
        DateTime? periodoInicio,
        DateTime? periodoFim,
        string? tipo,
        Guid? categoriaId,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (periodoInicio is DateTime inicio) query.Add($"PeriodoInicio={inicio:yyyy-MM-dd}");
        if (periodoFim is DateTime fim) query.Add($"PeriodoFim={fim:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(tipo)) query.Add($"Tipo={tipo}");
        if (categoriaId is Guid id) query.Add($"CategoriaId={id}");

        var uri = "api/movimentacoes" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);

        return await SendAsync<IReadOnlyList<MovimentacaoResponse>>(uri, cancellationToken) ?? [];
    }

    public async Task<FluxoCaixaResponse?> ObterFluxoCaixaAsync(
        DateTime periodoInicio,
        DateTime periodoFim,
        CancellationToken cancellationToken = default)
    {
        var uri = $"api/movimentacoes/fluxo-caixa?periodoInicio={periodoInicio:yyyy-MM-dd}&periodoFim={periodoFim:yyyy-MM-dd}";

        return await SendAsync<FluxoCaixaResponse>(uri, cancellationToken);
    }

    private async Task<T?> SendAsync<T>(string uri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        AddAuthorization(request);

        using var response = await httpClientFactory.CreateClient("FinanCoreApi").SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Movimentações API retornou {(int)response.StatusCode}.");

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    private void AddAuthorization(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(authService.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authService.AccessToken);
    }
}
