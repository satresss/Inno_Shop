using System.Net;
using System.Text.Json;
using ProductService.Application.Exceptions;

namespace ProductService.API.Middleware
{
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
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, title) = exception switch
            {
                ProductNotFoundException => (HttpStatusCode.NotFound, "Product not found"),
                UnauthorizedProductAccessException => (HttpStatusCode.Forbidden, "Unauthorized access to product"),
                ArgumentNullException => (HttpStatusCode.BadRequest, "Required argument is null"),
                ArgumentException => (HttpStatusCode.BadRequest, "Invalid argument"),
                _ => (HttpStatusCode.InternalServerError, "An error occurred while processing your request")
            };

            context.Response.StatusCode = (int)statusCode;

            var problemDetails = new
            {
                type = GetProblemDetailsType(statusCode),
                title = title,
                status = context.Response.StatusCode,
                detail = exception.Message,
                instance = context.Request.Path,
                traceId = context.TraceIdentifier
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(problemDetails, options);
            return context.Response.WriteAsync(json);
        }

        private static string GetProblemDetailsType(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                HttpStatusCode.BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                HttpStatusCode.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                HttpStatusCode.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
                _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };
        }
    }
}

