using System.Text;
using FinanCore.API.Middlewares;
using FinanCore.Application;
using FinanCore.Application.Abstractions;
using FinanCore.Application.Common;
using FinanCore.Domain.Entities;
using FinanCore.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "FinanCore API",
        Version = "v1",
        Description = "API REST do FinanCore - Sistema de Gestao Financeira Empresarial."
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
    throw new InvalidOperationException("JwtSettings:SecretKey não configurada (defina via User Secrets ou variável de ambiente).");

byte[] secretKeyBytes;
try
{
    secretKeyBytes = Convert.FromBase64String(jwtSettings.SecretKey);
}
catch (FormatException)
{
    secretKeyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
}

if (secretKeyBytes.Length < 32)
    throw new InvalidOperationException("JwtSettings:SecretKey deve ter ao menos 32 bytes (256 bits) para HMAC-SHA256. Gere uma nova chave com: dotnet user-secrets set JwtSettings:SecretKey \"$(python -c 'import secrets; print(secrets.token_urlsafe(32))')\"");

if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
    throw new InvalidOperationException("JwtSettings:Issuer não configurado.");

if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
    throw new InvalidOperationException("JwtSettings:Audience não configurado.");

builder.Services.AddSingleton<IOptions<JwtSettings>>(Options.Create(jwtSettings));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    byte[] secretKeyBytes;
    try
    {
        secretKeyBytes = Convert.FromBase64String(jwtSettings.SecretKey);
    }
    catch (FormatException)
    {
        secretKeyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    var jwtPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();

    options.DefaultPolicy = jwtPolicy;
    options.FallbackPolicy = jwtPolicy;
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FinanCore API v1");
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { status = "FinanCore.API running" }));

app.MapControllers();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<IFinanCoreDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedAdminInitializer");
        await SeedAdminInitializer.SeedAsync(dbContext, passwordHasher, app.Configuration, logger);
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("SeedAdminInitializer");
    logger.LogError(ex, "Erro ao executar SeedAdminInitializer durante inicialização.");
    throw;
}

app.Run();

public partial class Program
{
}
 