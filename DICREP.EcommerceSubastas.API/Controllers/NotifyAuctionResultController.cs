using DICREP.EcommerceSubastas.API.Filters;
using DICREP.EcommerceSubastas.Application.DTOs.FichaProducto;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.UseCases.FichaProducto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DICREP.EcommerceSubastas.API.Controllers
{
    using Serilog;

    [Route("api/[controller]")]
    [ApiController]
    public class NotifyAuctionResultController : ControllerBase
    {
        private readonly NotifyAuctionResultUseCase _notifyAuctionResultUseCase;
        private readonly ILogger _logger;

        public NotifyAuctionResultController(NotifyAuctionResultUseCase notifyAuctionResultUseCase)
        {
            _notifyAuctionResultUseCase = notifyAuctionResultUseCase;
            _logger = Log.ForContext<NotifyAuctionResultController>();
        }


        [AllowAnonymous]
        [HttpPost]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<ActionResult<ResponseDTO<int>>> NotifyAuctionResult(NotifyAuctionResultDto dto)
        {
            _logger.Information("Recibiendo resultado de subasta con ID {id_publicacion_bien}",
                       dto?.id_publicacion_bien);

            var result = await _notifyAuctionResultUseCase.ExecuteAsync(dto);

            if (!result.Success)
            {
                _logger.Warning("Error al procesar el bien con ID {ProductId}: {ErrorMessage}",
                   dto?.id_publicacion_bien,
                   result.Error?.Message);

                var statusCode = result.Error.HttpStatusCode ?? StatusCodes.Status500InternalServerError;
                return StatusCode(statusCode, result);
            }

            _logger.Information("Bien procesado correctamente para ID {ProductId}",
                       dto?.id_publicacion_bien);
            return result;
        }
    }
}
