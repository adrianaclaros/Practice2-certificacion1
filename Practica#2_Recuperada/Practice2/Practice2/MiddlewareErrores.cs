using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace Practice2
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
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción no controlada.");

                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                string mensajeDeError;
                switch (ex)
                {
                    case ArgumentNullException nullEx:
                        mensajeDeError = "Error: Argumento nulo.";
                        break;
                    case InvalidOperationException invalidOpEx:
                        mensajeDeError = "Error: Operación no válida.";
                        break;
                    case UnauthorizedAccessException unauthorizedEx:
                        mensajeDeError = "Error: Acceso no autorizado.";
                        break;
                    default:
                        mensajeDeError = "Ha ocurrido un error inesperado. Por favor, intente nuevamente más tarde.";
                        break;
                }

                await httpContext.Response.WriteAsync("Se produjo un error inesperado. Vuelva a intentarlo más tarde.");
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
