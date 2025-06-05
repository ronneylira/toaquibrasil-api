using System.Net;
using System.Text.Json;
using ToAquiBrasil.Api.Dtos;

namespace ToAquiBrasil.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var (statusCode, message) = exception switch
        {
            ArgumentException ex => (HttpStatusCode.BadRequest, ex.Message),
            KeyNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access"),
            InvalidOperationException ex => (HttpStatusCode.BadRequest, ex.Message),
            NotImplementedException => (HttpStatusCode.NotImplemented, "This functionality is not implemented"),
            // Add more exception mappings as needed
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };
        
        // Enhanced logging with request context
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var requestId = context.TraceIdentifier;
        
        _logger.LogError(
            exception,
            "Request {RequestMethod} {RequestPath} failed with status code {StatusCode}: {Message} (Request ID: {RequestId})", 
            requestMethod,
            requestPath,
            (int)statusCode, 
            exception.Message,
            requestId
        );
        
        // Create API response
        var response = CreateApiResponse(
            statusCode,
            message,
            exception,
            _environment.IsDevelopment() ? GetDetailedErrorInfo(exception, requestId) : null
        );
        
        context.Response.StatusCode = response.StatusCode;
        
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await context.Response.WriteAsync(jsonResponse);
    }

    private static ApiResponse<object> CreateApiResponse(
        HttpStatusCode statusCode, 
        string message, 
        Exception exception,
        object detailsForDevelopment = null)
    {
        var response = ApiResponse<object>.CreateError(
            message,
            (int)statusCode,
            exception.GetType().Name
        );
        
        // Add stack trace and additional details only in development mode
        if (detailsForDevelopment != null)
        {
            response.Data = detailsForDevelopment;
        }
        
        return response;
    }
    
    private static object GetDetailedErrorInfo(Exception exception, string requestId)
    {
        return new
        {
            Exception = exception.GetType().Name,
            Message = exception.Message,
            StackTrace = exception.StackTrace,
            InnerException = exception.InnerException?.Message,
            RequestId = requestId,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
} 