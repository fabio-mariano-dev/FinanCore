using System.Net.Http.Headers;
using System.Net.Http.Json;
using FinanCore.Web.Models;

namespace FinanCore.Web.Services;

public sealed class DashboardService(IHttpClientFactory httpClientFactory, AuthService authService)
{
    public async Task<DashboardResponse?> ObterAsync(
        DateTime? periodoInicio = null,
        DateTime? periodoFim = null,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("FinanCoreApi");
        if (!string.IsNullOrWhiteSpace(authService.AccessToken))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authService.AccessToken);
        }

        var query = new List<string>();
        if (periodoInicio is DateTime inicio) query.Add($"periodoInicio={inicio:yyyy-MM-dd}");
        if (periodoFim is DateTime fim) query.Add($"periodoFim={fim:yyyy-MM-dd}");

        var uri = "api/dashboard" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);

        var response = await client.GetAsync(uri, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Dashboard API retornou {(int)response.StatusCode}.");

        return await response.Content.ReadFromJsonAsync<DashboardResponse>(cancellationToken);
    }
}
