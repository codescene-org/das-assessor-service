using Microsoft.AspNetCore.Builder;

namespace SFA.DAS.AssessorService.Web.Extensions
{
    public static class SecurityHeadersExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; font-src *;img-src * data:; script-src *; style-src *;");
                context.Response.Headers.Add("Referrer-Policy", "strict-origin");
                await next();
            });
            
            return app;
        }
    }
}