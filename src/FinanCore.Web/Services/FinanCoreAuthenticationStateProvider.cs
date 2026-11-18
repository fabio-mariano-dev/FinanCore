using System.Security.Claims;
using FinanCore.Web.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace FinanCore.Web.Services;

public sealed class FinanCoreAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());
    private ClaimsPrincipal currentUser = Anonymous;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(currentUser));

    public void MarkAuthenticated(UsuarioLogadoResponse usuario, string accessToken)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil),
            new Claim("access_token", accessToken)
        };

        currentUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "FinanCore")).Clone() as ClaimsPrincipal
            ?? new ClaimsPrincipal(new ClaimsIdentity(claims, "FinanCore"));
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void MarkLoggedOut()
    {
        currentUser = Anonymous;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
