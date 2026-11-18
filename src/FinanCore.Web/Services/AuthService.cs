using System.Net;
using System.Net.Http.Json;
using FinanCore.Web.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FinanCore.Web.Services;

public sealed class AuthService(
    IHttpClientFactory httpClientFactory,
    FinanCoreAuthenticationStateProvider authenticationStateProvider,
    ProtectedSessionStorage sessionStorage)
{
    private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
    private readonly ProtectedSessionStorage sessionStorage = sessionStorage;

    private const string AccessTokenStorageKey = "financore.access-token";
    private const string UsuarioStorageKey = "financore.usuario";

    public string? AccessToken { get; private set; }
    public UsuarioLogadoResponse? Usuario { get; private set; }

    public async Task LogoutAsync()
    {
        AccessToken = null;
        Usuario = null;
        await sessionStorage.DeleteAsync(AccessTokenStorageKey);
        await sessionStorage.DeleteAsync(UsuarioStorageKey);
        authenticationStateProvider.MarkLoggedOut();
    }

    public async Task RestoreAsync()
    {
        var accessToken = await sessionStorage.GetAsync<string>(AccessTokenStorageKey);
        var usuario = await sessionStorage.GetAsync<UsuarioLogadoResponse>(UsuarioStorageKey);

        if (!accessToken.Success || !usuario.Success || string.IsNullOrWhiteSpace(accessToken.Value) || usuario.Value is null)
            return;

        AccessToken = accessToken.Value;
        Usuario = usuario.Value;
        authenticationStateProvider.MarkAuthenticated(Usuario, AccessToken);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("FinanCoreApi");
        using var response = await client.PostAsJsonAsync("api/auth/login", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
            if (loginResponse is null || string.IsNullOrWhiteSpace(loginResponse.AccessToken))
                return (false, "Não foi possível concluir o login. Tente novamente.");

            AccessToken = loginResponse.AccessToken;
            Usuario = loginResponse.Usuario;
            await sessionStorage.SetAsync(AccessTokenStorageKey, AccessToken);
            await sessionStorage.SetAsync(UsuarioStorageKey, Usuario);
            authenticationStateProvider.MarkAuthenticated(Usuario, AccessToken);
            return (true, null);
        }

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized =>
                (false, "E-mail ou senha inválidos."),
            _ => (false, "Não foi possível conectar ao sistema. Tente novamente em instantes.")
        };
    }
}
