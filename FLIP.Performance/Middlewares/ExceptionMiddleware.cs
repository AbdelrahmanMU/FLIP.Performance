using FLIP.Application.Models;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace FLIP.API.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception Occurred");

            context.Response.ContentType = "application/json";

            var statusCode = ex switch
            {
                ValidationException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                _ => (int)HttpStatusCode.InternalServerError
            };
            context.Response.StatusCode = statusCode;

            var errorCode = ex switch
            {
                ValidationException => "VALIDATION_ERROR",
                UnauthorizedAccessException => "UNAUTHORIZED",
                KeyNotFoundException => "NOT_FOUND",
                _ => "INTERNAL_SERVER_ERROR"
            };

            var response = new ResponseVM<object>
            {
                Status = "error",
                Error = new ApiError
                {
                    ErrorCode = errorCode,
                    ErrorDescription = ex is ValidationException ? "Validation failed." : ex.Message,
                    SubError = ex.InnerException?.Message,
                    Validations = ex is ValidationException vex
                        ? [.. vex.Errors
                            .GroupBy(e => e.PropertyName)
                            .Select(g => new ValidationError
                            {
                                PropertyName = g.Key,
                                ValidationMessage = g.Select(x => x.ErrorMessage).Distinct().ToList()
                            })]
                        : []
                },
                Data = new { }
            };

            var json = JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(json);
        }
    }
}