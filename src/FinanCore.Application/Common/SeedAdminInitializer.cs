using FinanCore.Application.Abstractions;
using FinanCore.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FinanCore.Application.Common;

public static class SeedAdminInitializer
{
    public static async Task SeedAsync(
        IFinanCoreDbContext dbContext,
        IPasswordHasher<Usuario> passwordHasher,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var email = configuration["Seed:AdminEmail"];
        var senha = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            return;

        if (await dbContext.Usuarios.AnyAsync(u => u.Email == email, cancellationToken))
            return;

        var perfilNome = configuration["Seed:AdminPerfilNome"];
        perfilNome = string.IsNullOrWhiteSpace(perfilNome) ? "Administrador" : perfilNome;

        var perfil = await dbContext.Perfis.FirstOrDefaultAsync(p => p.Nome == perfilNome, cancellationToken);
        if (perfil is null)
        {
            perfil = new Perfil(perfilNome);
            dbContext.Perfis.Add(perfil);
        }

        var nome = configuration["Seed:AdminNome"];
        nome = string.IsNullOrWhiteSpace(nome) ? "Administrador" : nome;

        var senhaHash = passwordHasher.HashPassword(null!, senha);
        var usuario = new Usuario(nome, email, senhaHash, perfil.Id);
        dbContext.Usuarios.Add(usuario);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Usuário administrador inicial criado via seed ({Email}).", email);
    }
}
