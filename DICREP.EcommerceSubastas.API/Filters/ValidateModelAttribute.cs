namespace DICREP.EcommerceSubastas.API.Filters
{
    using DICREP.EcommerceSubastas.Application.DTOs.Responses;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System.Linq;
    using Serilog;

    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly ILogger _logger;
        public ValidateModelAttribute()
        {
            _logger = Log.ForContext<ValidateModelAttribute>();
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var errorMessages = errors.SelectMany(e => e.Value).ToList();

                _logger.Warning("Validación de modelo fallida: {Errors}", string.Join("; ", errorMessages));

                var response = new ResponseDTO<int>
                {
                    Success = false,
                    Data = 0,
                    Message = "Ha ocurrido un error al recibir la ficha del producto",
                    Error = new ErrorResponseDto
                    {
                        ErrorCode = 40016,
                        Message = string.Join("; ", errorMessages),
                        HttpStatusCode = 400
                    }
                };

                context.Result = new BadRequestObjectResult(response);
            }
        }
    }

}
