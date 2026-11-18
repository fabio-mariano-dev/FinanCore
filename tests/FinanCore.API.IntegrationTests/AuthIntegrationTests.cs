using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FinanCore.Application.Common;
using FinanCore.Application.DTOs.Auth;
using FinanCore.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FinanCore.API.IntegrationTests;

public class AuthIntegrationTests(FinanCoreApiFactory factory) : IClassFixture<FinanCoreApiFactory>
{
    private const string SenhaValida = "SenhaForte@123";

    private async Task<(Guid UsuarioId, string Email)> SeedUsuarioAsync(bool ativo = true, string perfilNome = "Financeiro")
    {
        using var scope = factory.CreateDbScope();
        var db = scope.GetDbContext();
        var hasher = new PasswordHasher<Usuario>();

        var email = $"usuario.{Guid.NewGuid():N}@financore.com";
        var perfil = new Perfil($"{perfilNome}-{Guid.NewGuid():N}");
        var senhaHash = hasher.HashPassword(null!, SenhaValida);
        var usuario = new Usuario("Usuário de Teste", email, senhaHash, perfil.Id);
        if (!ativo)
            usuario.Desativar();

        db.Perfis.Add(perfil);
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        return (usuario.Id, email);
    }

    private JwtSettings ObterJwtSettings()
    {
        using var scope = factory.CreateDbScope();
        return scope.ServiceProvider.GetRequiredService<IConfiguration>().GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings não configurado no ambiente de teste.");
    }

    private static string ConstruirToken(JwtSettings settings, DateTime notBefore, DateTime expires, string? secretKeyOverride = null)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) };
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyOverride ?? settings.SecretKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: notBefore,
            expires: expires,
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task Login_CredenciaisValidas_DeveRetornar200ComTokenEDadosDoUsuario()
    {
        var (usuarioId, email) = await SeedUsuarioAsync();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, SenhaValida));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
        Assert.Equal(usuarioId, body.Usuario.Id);
        Assert.Equal(email, body.Usuario.Email);
    }

    [Fact]
    public async Task Login_SenhaIncorreta_DeveRetornar401()
    {
        var (_, email) = await SeedUsuarioAsync();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "SenhaErrada@123"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_EmailInexistente_DeveRetornar401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest($"inexistente.{Guid.NewGuid():N}@financore.com", SenhaValida));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UsuarioInativo_DeveRetornar401()
    {
        var (_, email) = await SeedUsuarioAsync(ativo: false);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, SenhaValida));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_RequestInvalido_DeveRetornar400()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("", ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_SemToken_DeveRetornar401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_ComTokenValido_DevePermitirAcesso()
    {
        var (_, email) = await SeedUsuarioAsync();
        var client = factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, SenhaValida));
        var loginBody = await login.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);
        var response = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_ComTokenExpirado_DeveRetornar401()
    {
        var settings = ObterJwtSettings();
        var expirado = ConstruirToken(settings, notBefore: DateTime.UtcNow.AddMinutes(-10), expires: DateTime.UtcNow.AddMinutes(-1));

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expirado);
        var response = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_ComAssinaturaInvalida_DeveRetornar401()
    {
        var settings = ObterJwtSettings();
        var chaveErrada = "chave-invalida-para-teste-com-32-bytes-no-minimo!!";
        var tokenComAssinaturaInvalida = ConstruirToken(settings, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(30), secretKeyOverride: chaveErrada);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenComAssinaturaInvalida);
        var response = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Seed_SemConfiguracaoExplicita_NaoDeveCriarUsuarioAdministrador()
    {
        using var scope = factory.CreateDbScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        Assert.True(string.IsNullOrWhiteSpace(configuration["Seed:AdminEmail"]));
        Assert.True(string.IsNullOrWhiteSpace(configuration["Seed:AdminPassword"]));

        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@financore.com", "qualquer-senha"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
