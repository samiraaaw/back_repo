
namespace DICREP.EcommerceSubastas.API.Middlewares
{
    using DICREP.EcommerceSubastas.Application.Exceptions;
    using Serilog;
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
            _logger = Log.ForContext<ExceptionHandlingMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ReglaNegocioException ex)
            {
                _logger.Warning(ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest; // 400 Bad Request
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (DatosFaltantesException ex)
            {
                _logger.Warning(ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest; // 400 Bad Request
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.Warning(ex.Message);
                context.Response.StatusCode = StatusCodes.Status404NotFound; // 404 Not Found
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (EntityAlreadyExistsException ex)
            {
                _logger.Warning(ex.Message);
                context.Response.StatusCode = StatusCodes.Status409Conflict; // 409 Conflict
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (InvalidCredentialsException ex)
            {
                _logger.Warning(ex.Message);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized; // 401 Unauthorized
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Excepción no controlada ocurrida.");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError; // 500 Error Interno del Servidor
                await context.Response.WriteAsJsonAsync(new { error = "Ocurrió un error inesperado en el servidor: " + ex.Message });
            }
        }
    }

}

