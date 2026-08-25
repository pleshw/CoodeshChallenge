using System.Text.Json;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware
{
    /// <summary>
    /// Converts exceptions that escape the MediatR pipeline into the API's
    /// standardized { type, error, detail } error shape (see .doc/general-api.md).
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                await WriteResponseAsync(context, StatusCodes.Status400BadRequest, "ValidationError",
                    "Invalid input data", string.Join(" ", ex.Errors.Select(e => e.ErrorMessage)));
            }
            catch (DomainException ex)
            {
                await WriteResponseAsync(context, StatusCodes.Status400BadRequest, "BusinessRuleViolation",
                    "Business rule violation", ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                await WriteResponseAsync(context, StatusCodes.Status404NotFound, "ResourceNotFound",
                    "Resource not found", ex.Message);
            }
        }

        private static Task WriteResponseAsync(HttpContext context, int statusCode, string type, string error, string detail)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new { type, error, detail };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
