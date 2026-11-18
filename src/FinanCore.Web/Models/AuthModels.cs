namespace FinanCore.Web.Models;

public sealed record LoginRequest(string Email, string Senha);

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    UsuarioLogadoResponse Usuario);

public sealed record UsuarioLogadoResponse(
    Guid Id,
    string Nome,
    string Email,
    string Perfil);
