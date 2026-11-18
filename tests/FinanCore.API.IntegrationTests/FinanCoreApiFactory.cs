using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using FinanCore.Application.Abstractions;
using FinanCore.Application.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinanCore.API.IntegrationTests;

public class FinanCoreApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("JwtSettings:SecretKey", "integration-test-secret-key-with-at-least-32-bytes");
        builder.UseSetting("JwtSettings:Issuer", "FinanCore.API");
        builder.UseSetting("JwtSettings:Audience", "FinanCore.Clients");
        builder.UseSetting("JwtSettings:ExpirationMinutes", "60");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "integration-test-secret-key-with-at-least-32-bytes",
                ["JwtSettings:Issuer"] = "FinanCore.API",
                ["JwtSettings:Audience"] = "FinanCore.Clients",
                ["JwtSettings:ExpirationMinutes"] = "60",
                ["Seed:AdminEmail"] = null,
                ["Seed:AdminPassword"] = null
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IOptions<JwtSettings>>();
            services.AddSingleton<IOptions<JwtSettings>>(Options.Create(new JwtSettings
            {
                SecretKey = "integration-test-secret-key-with-at-least-32-bytes",
                Issuer = "FinanCore.API",
                Audience = "FinanCore.Clients",
                ExpirationMinutes = 60
            }));

        });
    }
}

public static class FinanCoreApiFactoryExtensions
{
    public static IServiceScope CreateDbScope(this FinanCoreApiFactory factory) => factory.Services.CreateScope();

    public static IFinanCoreDbContext GetDbContext(this IServiceScope scope) => scope.ServiceProvider.GetRequiredService<IFinanCoreDbContext>();

    public static HttpClient CreateAuthenticatedClient(this FinanCoreApiFactory factory)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("integration-test-secret-key-with-at-least-32-bytes"));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "FinanCore.API",
            audience: "FinanCore.Clients",
            claims: [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())],
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            new JwtSecurityTokenHandler().WriteToken(token));
        return client;
    }
}
