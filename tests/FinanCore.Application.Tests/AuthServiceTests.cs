using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FinanCore.Application.Common;
using FinanCore.Application.DTOs.Auth;
using FinanCore.Application.Services;
using FinanCore.Application.Validators.Auth;
using FinanCore.Domain.Entities;
using FinanCore.Domain.Enums;
using FinanCore.Domain.Exceptions;
using FinanCore.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FinanCore.Application.Tests;

public class AuthServiceTests
{
    private const string SenhaValida = "SenhaForte@123";

    private static FinanCoreDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanCoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FinanCoreDbContext(options);
    }

    private static AuthService CriarService(FinanCoreDbContext dbContext, PasswordHasher<Usuario> hasher, TimeProvider? timeProvider = null)
        => new(
            dbContext,
            hasher,
            Options.Create(new JwtSettings
            {
                SecretKey = "chave-de-teste-com-pelo-menos-32-bytes-1234567890",
                Issuer = "FinanCore.Tests",
                Audience = "FinanCore.Tests.Clients",
                ExpirationMinutes = 60
            }),
            new LoginRequestValidator(),
            timeProvider ?? TimeProvider.System);

    private static async Task<(Usuario Usuario, Perfil Perfil)> SeedUsuarioAsync(
        FinanCoreDbContext dbContext,
        PasswordHasher<Usuario> hasher,
        bool ativo = true,
        string perfilNome = "Administrador",
        string email = "usuario@financore.com")
    {
        var perfil = new Perfil(perfilNome);
        var senhaHash = hasher.HashPassword(null!, SenhaValida);
        var usuario = new Usuario("Usuário Teste", email, senhaHash, perfil.Id);
        if (!ativo)
            usuario.Desativar();

        dbContext.Perfis.Add(perfil);
        dbContext.Usuarios.Add(usuario);
        await dbContext.SaveChangesAsync();

        return (usuario, perfil);
    }

    [Fact]
    public async Task LoginAsync_CredenciaisValidas_DeveRetornarTokenComClaimsEsperadas()
    {
        await using var dbContext = CriarDbContext();
        var hasher = new PasswordHasher<Usuario>();
        var (usuario, perfil) = await SeedUsuarioAsync(dbContext, hasher);
        var service = CriarService(dbContext, hasher);

        var response = await service.LoginAsync(new LoginRequest(usuario.Email, SenhaValida));

        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.Equal(usuario.Id, response.Usuario.Id);
        Assert.Equal(usuario.Email, response.Usuario.Email);
        Assert.Equal(perfil.Nome, response.Usuario.Perfil);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);
        Assert.Equal(usuario.Id.ToString(), token.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(usuario.Email, token.Claims.Single(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal(perfil.Nome, token.Claims.Single(c => c.Type == ClaimTypes.Role).Value);
        Assert.Equal("FinanCore.Tests", token.Issuer);
        Assert.Contains("FinanCore.Tests.Clients", token.Audiences);
        Assert.DoesNotContain(token.Claims, c => c.Value == usuario.SenhaHash);
    }

    [Fact]
    public async Task LoginAsync_SenhaIncorreta_DeveLancarCredenciaisInvalidas()
    {
        await using var dbContext = CriarDbContext();
        var hasher = new PasswordHasher<Usuario>();
        var (usuario, _) = await SeedUsuarioAsync(dbContext, hasher);
        var service = CriarService(dbContext, hasher);

        await Assert.ThrowsAsync<CredenciaisInvalidasException>(
            () => service.LoginAsync(new LoginRequest(usuario.Email, "SenhaErrada@123")));
    }

    [Fact]
    public async Task LoginAsync_EmailInexistente_DeveLancarCredenciaisInvalidas()
    {
        await using var dbContext = CriarDbContext();
        var hasher = new PasswordHasher<Usuario>();
        var service = CriarService(dbContext, hasher);

        await Assert.ThrowsAsync<CredenciaisInvalidasException>(
            () => service.LoginAsync(new LoginRequest("inexistente@financore.com", SenhaValida)));
    }

    [Fact]
    public async Task LoginAsync_UsuarioInativo_DeveLancarCredenciaisInvalidas()
    {
        await using var dbContext = CriarDbContext();
        var hasher = new PasswordHasher<Usuario>();
        var (usuario, _) = await SeedUsuarioAsync(dbContext, hasher, ativo: false);
        var service = CriarService(dbContext, hasher);

        await Assert.ThrowsAsync<CredenciaisInvalidasException>(
            () => service.LoginAsync(new LoginRequest(usuario.Email, SenhaValida)));
    }

    [Fact]
    public async Task LoginAsync_RequestInvalido_DeveLancarValidationException()
    {
        await using var dbContext = CriarDbContext();
        var hasher = new PasswordHasher<Usuario>();
        var service = CriarService(dbContext, hasher);

        await Assert.ThrowsAsync<ValidationException>(() => service.LoginAsync(new LoginRequest("", "")));
    }
}
