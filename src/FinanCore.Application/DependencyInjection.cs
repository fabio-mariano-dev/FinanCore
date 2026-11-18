using FinanCore.Application.Abstractions;
using FinanCore.Application.Services;
using FinanCore.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FinanCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IFornecedorService, FornecedorService>();
        services.AddScoped<IContaPagarService, ContaPagarService>();
        services.AddScoped<IContaReceberService, ContaReceberService>();
        services.AddScoped<IMovimentacaoFinanceiraService, MovimentacaoFinanceiraService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

        return services;
    }
}
