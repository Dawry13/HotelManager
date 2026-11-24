using System.Net;
using System.Text.Json;
using HotelManager.Business.Exceptions;

namespace HotelManager.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (BusinessException ex)
        {
            // Reglas de negocio: el mensaje es apto para mostrarse al cliente.
            var status = ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase)
                ? HttpStatusCode.NotFound
                : HttpStatusCode.BadRequest;

            _logger.LogWarning("Regla de negocio: {Mensaje}", ex.Message);
            await EscribirProblema(context, status, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado: {Mensaje}", ex.Message);
            await EscribirProblema(context, HttpStatusCode.InternalServerError, "Ocurrió un error inesperado.");
        }
    }

    private static Task EscribirProblema(HttpContext context, HttpStatusCode status, string titulo)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        var problem = new { status = (int)status, title = titulo };
        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
