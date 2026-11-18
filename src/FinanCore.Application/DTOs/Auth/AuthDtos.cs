namespace FinanCore.Application.DTOs.Auth;

public record LoginRequest(string Email, string Senha);

public record UsuarioLogadoResponse(Guid Id, string Nome, string Email, string Perfil);

public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, UsuarioLogadoResponse Usuario);
