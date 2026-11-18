using System.Net;
using System.Text.Json;
using FinanCore.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.API.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "Um ou mais erros de validação ocorreram.",
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),

            RegraNegocioException regraNegocioException => (
                HttpStatusCode.BadRequest,
                regraNegocioException.Message,
                null),

            CredenciaisInvalidasException credenciaisInvalidasException => (
                HttpStatusCode.Unauthorized,
                credenciaisInvalidasException.Message,
                null),

            KeyNotFoundException keyNotFoundException => (
                HttpStatusCode.NotFound,
                keyNotFoundException.Message,
                null),

            DbUpdateConcurrencyException => (
                HttpStatusCode.Conflict,
                "O recurso foi alterado por outra operação concorrente. Recarregue os dados e tente novamente.",
                null),

            DbUpdateException => (
                HttpStatusCode.Conflict,
                "A operação conflita com o estado atual dos dados.",
                null),

            _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado ao processar a requisição.", null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Erro não tratado ao processar {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            logger.LogWarning(exception, "Erro de negócio/validação ao processar {Method} {Path}: {Message}", context.Request.Method, context.Request.Path, exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Instance = context.Request.Path
        };

        if (errors is not null)
            problemDetails.Extensions["errors"] = errors;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
