namespace eShop.Startup.Middlewares
{
    using Microsoft.AspNetCore.Builder;

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMiddleware(
            this IApplicationBuilder builder
        ) => builder.UseMiddleware<CorsMiddleware>();
    }
}