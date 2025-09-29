using DICREP.EcommerceSubastas.API.Security;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using System.Text.Json;

namespace DICREP.EcommerceSubastas.API.Middlewares
{
    public class ApiKeyMiddleware
    {
        
        private readonly RequestDelegate _next;
        public IApiKeyValidation Validator { get; }

        public ApiKeyMiddleware(RequestDelegate next, IApiKeyValidation validator)
        {
            _next = next;
            Validator = validator;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            if (!ctx.Request.Headers.TryGetValue(Constants.ApiKeyHeaderName, out var key) ||
                !Validator.IsValid(key))
            {
                var isMissing = key.Count == 0;

                ctx.Response.StatusCode = isMissing
                    ? StatusCodes.Status400BadRequest
                    : StatusCodes.Status401Unauthorized;

                ctx.Response.ContentType = "application/json";

                var response = new ResponseDTO<int>
                {
                    Success = false,
                    Data = 0,
                    Message = "Ha ocurrido un error al recibir la ficha del producto",
                    Error = new ErrorResponseDto
                    {
                        ErrorCode = isMissing ? 40101 : 40102,
                        Message = isMissing
                            ? "Falta el header 'X-API-Key'."
                            : "API Key inválida o no autorizada.",
                        HttpStatusCode = ctx.Response.StatusCode
                    }
                };

                // Serialización con camelCase
                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                await ctx.Response.WriteAsync(json);
                return;

            }

            await _next(ctx);
        }
    }
}
