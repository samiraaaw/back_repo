using DICREP.EcommerceSubastas.API.Filters;
using DICREP.EcommerceSubastas.Application.DTOs.FichaProducto;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.UseCases.FichaProducto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DICREP.EcommerceSubastas.API.Controllers
{
    using Serilog;

    [Route("api/[controller]")]
    [ApiController]
    public class ReceiveFichaProducto : ControllerBase
    {

        private readonly ReceiveFichaUseCase _receiveFichaUseCase;
        private readonly ILogger _logger;

        public ReceiveFichaProducto( ReceiveFichaUseCase receivePrendasCLUseCase)
        {
            _receiveFichaUseCase = receivePrendasCLUseCase;
            _logger = Log.ForContext<ReceiveFichaProducto>();
        }


        [AllowAnonymous]
        [HttpPost]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<ActionResult<ResponseDTO<int>>> ReceiveFichaProductoPOST(ReceiveFichaDto dto)
        {
            _logger.Information("Recibiendo ficha de producto con ID {ProductId}",
                       dto?.ficha_producto?.detalle_bien?.id_publicacion_bien);

            var result = await _receiveFichaUseCase.ExecuteAsync(dto);

            if (!result.Success)
            {
                _logger.Warning("Error al procesar ficha de producto con ID {ProductId}: {ErrorMessage}",
                   dto?.ficha_producto?.detalle_bien?.id_publicacion_bien,
                   result.Error?.Message);

                var statusCode = result.Error.HttpStatusCode ?? StatusCodes.Status500InternalServerError;
                return StatusCode(statusCode, result);
            }

            _logger.Information("Ficha de producto procesada correctamente para ID {ProductId}",
                       dto?.ficha_producto?.detalle_bien?.id_publicacion_bien);
            return result;
        }
    }
}
