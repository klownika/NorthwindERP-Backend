using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NorthwindERP.Core.Exceptions;

namespace NorthwindERP.API.Middlewares;

/// <summary>
/// Atrapa TODAS las excepciones y las convierte en una respuesta JSON clara (ProblemDetails).
/// Así los controladores no necesitan try/catch y el frontend siempre recibe el mismo formato:
/// { "status": 400, "title": "...", "detail": "Stock insuficiente para el producto 5" }
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (status, title, detail) = Map(ex);

        if (status >= 500)
            _logger.LogError(ex, "Error no controlado en {Path}", context.Request.Path);
        else
            _logger.LogWarning("Petición rechazada ({Status}) en {Path}: {Detail}", status, context.Request.Path, detail);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }

    private (int Status, string Title, string? Detail) Map(Exception ex)
    {
        switch (ex)
        {
            case NotFoundException:
                return (StatusCodes.Status404NotFound, "No encontrado", ex.Message);
            case BusinessRuleException:
                return (StatusCodes.Status400BadRequest, "Regla de negocio", ex.Message);
            case UnauthorizedAccessException:
                return (StatusCodes.Status401Unauthorized, "No autorizado", ex.Message);
        }

        // Errores que vienen de PostgreSQL / Supabase (directos o envueltos por EF Core)
        var pg = ex as PostgresException ?? ex.InnerException as PostgresException;
        if (pg is not null)
        {
            return pg.SqlState switch
            {
                // RAISE EXCEPTION dentro de tus funciones RPC (ej: "Stock insuficiente")
                "P0001" => (400, "Operación rechazada por la base de datos", pg.MessageText),
                PostgresErrorCodes.ForeignKeyViolation => (409, "Conflicto de integridad referencial",
                    "El registro está relacionado con otros datos que no existen o que dependen de él. " + pg.Detail),
                PostgresErrorCodes.UniqueViolation => (409, "Registro duplicado", pg.Detail ?? pg.MessageText),
                PostgresErrorCodes.NotNullViolation => (400, "Falta un dato obligatorio",
                    $"La columna '{pg.ColumnName}' de la tabla '{pg.TableName}' no acepta valores nulos."),
                PostgresErrorCodes.CheckViolation => (400, "Valor no permitido", pg.MessageText),
                PostgresErrorCodes.UndefinedFunction => (500, "Función RPC no encontrada",
                    "La función no existe o sus parámetros no coinciden: " + pg.MessageText),
                PostgresErrorCodes.UndefinedTable => (500, "Tabla o vista no encontrada", pg.MessageText),
                PostgresErrorCodes.UndefinedColumn => (500, "Columna no encontrada", pg.MessageText),
                _ => (500, "Error de base de datos", _env.IsDevelopment() ? pg.MessageText : null)
            };
        }

        return (StatusCodes.Status500InternalServerError, "Error interno del servidor",
            _env.IsDevelopment() ? ex.Message : null);
    }
}
