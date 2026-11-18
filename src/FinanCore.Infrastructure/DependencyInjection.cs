using FinanCore.Application.Abstractions;
using FinanCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FinanCoreDb")
            ?? throw new InvalidOperationException("Connection string 'FinanCoreDb' não configurada.");

        services.AddDbContext<FinanCoreDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IFinanCoreDbContext>(sp => sp.GetRequiredService<FinanCoreDbContext>());

        return services;
    }
}
