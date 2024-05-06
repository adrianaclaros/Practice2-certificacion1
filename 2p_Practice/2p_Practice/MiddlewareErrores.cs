using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace _2p_Practice
{
    public class MiddlewareErrores
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MiddlewareErrores> _logger;

        public MiddlewareErrores(RequestDelegate next, ILogger<MiddlewareErrores> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                // Ejecuta el siguiente middleware en la cadena de solicitud
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // Registra el error en el registro
                _logger.LogError(ex, "Unhandled exception occurred.");

                // Establece el código de estado en 500 (Error interno del servidor)
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                // Escribe un mensaje de error en el cuerpo de la respuesta
                await httpContext.Response.WriteAsync("An unexpected error occurred. Please try again later.");
            }
        }
    }

    public static class MiddlewareErroresExtensions
    {
        public static IApplicationBuilder UseMiddlewareErrores(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MiddlewareErrores>();
        }
    }
}
