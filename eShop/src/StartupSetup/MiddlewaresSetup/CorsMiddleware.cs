namespace eShop.Startup.Middlewares
{
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;

    public class CorsMiddleware
    {
        private readonly RequestDelegate _next;

        public CorsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public virtual Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add(
                "Access-Control-Allow-Origin", "*"
            );
            context.Response.Headers.Add(
                "Access-Control-Allow-Headers", "Content-Type, Accept"
            );
            context.Response.Headers.Add(
                "Access-Control-Allow-Methods", "GET,POST,PUT"
            );

            return _next(context);
        }
    }
}
