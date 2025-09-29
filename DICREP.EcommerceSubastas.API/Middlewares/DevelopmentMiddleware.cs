using DICREP.EcommerceSubastas.API.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace DICREP.EcommerceSubastas.API.Middlewares
{
    public class DevelopmentMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;

        public DevelopmentMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<DevelopmentOnlyAttribute>() != null
                && _env.EnvironmentName != "Development")
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            await _next(context);
        }

    }
}
