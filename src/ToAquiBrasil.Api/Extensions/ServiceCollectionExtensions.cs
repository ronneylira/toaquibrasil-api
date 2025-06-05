using ToAquiBrasil.Api.Middleware;

namespace ToAquiBrasil.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
} 