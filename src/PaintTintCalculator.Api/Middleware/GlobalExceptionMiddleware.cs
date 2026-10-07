using System.Net;
using System.Text.Json;
using PaintTintCalculator.Domain.Exceptions;

namespace PaintTintCalculator.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var code = "INTERNAL_SERVER_ERROR";
        var message = "An unexpected error occurred. Please try again later.";

        switch (exception)
        {
            case TintLimitExceededException ex:
                statusCode = HttpStatusCode.BadRequest;
                code = ex.Code;
                message = ex.Message;
                _logger.LogWarning(ex, "Tint limit exceeded: {Message}", ex.Message);
                break;

            case FormulaNotFoundException ex:
                statusCode = HttpStatusCode.NotFound;
                code = ex.Code;
                message = ex.Message;
                _logger.LogWarning(ex, "Formula not found: {Message}", ex.Message);
                break;

            case InvalidCanSizeException ex:
                statusCode = HttpStatusCode.BadRequest;
                code = ex.Code;
                message = ex.Message;
                _logger.LogWarning(ex, "Invalid can size: {Message}", ex.Message);
                break;

            case DomainException ex:
                statusCode = HttpStatusCode.BadRequest;
                code = ex.Code;
                message = ex.Message;
                _logger.LogWarning(ex, "Domain exception: {Message}", ex.Message);
                break;

            case KeyNotFoundException ex:
                statusCode = HttpStatusCode.NotFound;
                code = "NOT_FOUND";
                message = ex.Message;
                _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
                break;

            case ArgumentException ex:
                statusCode = HttpStatusCode.BadRequest;
                code = "INVALID_ARGUMENT";
                message = ex.Message;
                _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
                break;

            default:
                _logger.LogError(exception, "Unhandled server error occurred.");
                // Conceal internal exception message and stack trace from client
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            code,
            message,
            statusCode = (int)statusCode
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

