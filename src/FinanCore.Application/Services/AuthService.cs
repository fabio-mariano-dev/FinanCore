using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanCore.Application.Abstractions;
using FinanCore.Application.Common;
using FinanCore.Application.DTOs.Auth;
using FinanCore.Domain.Entities;
using FinanCore.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinanCore.Application.Services;

public class AuthService(
    IFinanCoreDbContext dbContext,
    IPasswordHasher<Usuario> passwordHasher,
    IOptions<JwtSettings> jwtSettings,
    IValidator<LoginRequest> loginValidator,
    TimeProvider timeProvider) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (usuario is null || !usuario.Ativo)
            throw new CredenciaisInvalidasException();

        var resultado = passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, request.Senha);
        if (resultado == PasswordVerificationResult.Failed)
            throw new CredenciaisInvalidasException();

        var perfil = await dbContext.Perfis.FirstOrDefaultAsync(p => p.Id == usuario.PerfilId, cancellationToken)
            ?? throw new CredenciaisInvalidasException();

        var (accessToken, expiraEmUtc) = GerarToken(usuario, perfil);

        return new LoginResponse(
            accessToken,
            expiraEmUtc,
            new UsuarioLogadoResponse(usuario.Id, usuario.Nome, usuario.Email, perfil.Nome));
    }

    private (string AccessToken, DateTime ExpiresAtUtc) GerarToken(Usuario usuario, Perfil perfil)
    {
        var settings = jwtSettings.Value;

        if (string.IsNullOrWhiteSpace(settings.SecretKey))
            throw new InvalidOperationException("JwtSettings:SecretKey não configurada para geração de token.");

        var agoraUtc = timeProvider.GetUtcNow().UtcDateTime;
        var expiraEmUtc = agoraUtc.AddMinutes(settings.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, perfil.Nome),
        };

        byte[] secretKeyBytes;
        try
        {
            secretKeyBytes = Convert.FromBase64String(settings.SecretKey);
        }
        catch (FormatException)
        {
            secretKeyBytes = Encoding.UTF8.GetBytes(settings.SecretKey);
        }

        var chaveAssinatura = new SymmetricSecurityKey(secretKeyBytes);
        var credenciais = new SigningCredentials(chaveAssinatura, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: agoraUtc,
            expires: expiraEmUtc,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEmUtc);
    }
}
