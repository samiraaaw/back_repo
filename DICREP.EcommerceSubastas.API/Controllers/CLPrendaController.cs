// Controllers/CLPrendaController.cs
using DICREP.EcommerceSubastas.API.Filters;
using DICREP.EcommerceSubastas.Application.DTOs.CLPrenda;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.UseCases.CLPrenda;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DICREP.EcommerceSubastas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CLPrendaController : ControllerBase
    {
        private readonly CLPrendaUpdateUseCase _clPrendaUpdateUseCase;
        private readonly Serilog.ILogger _logger;

        public CLPrendaController(CLPrendaUpdateUseCase clPrendaUpdateUseCase)
        {
            _clPrendaUpdateUseCase = clPrendaUpdateUseCase;
            _logger = Log.ForContext<CLPrendaController>();
        }

        /// <summary>
        /// Actualiza incremento y/o comisión de una prenda con auditoría
        /// </summary>
        /// <param name="request">Datos de actualización de la prenda</param>
        /// <returns>Información de la prenda actualizada</returns>
        [HttpPut("UpdateIncrementoComision")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO<CLPrendaUpdateResponseDTO>>> UpdateIncrementoComision(
            [FromBody] CLPrendaUpdateRequestDTO request)
        {
            _logger.Information("Recibiendo solicitud de actualización para prenda {CLPrendaCod}",
                request?.CLPrendaCod);

            var result = await _clPrendaUpdateUseCase.ExecuteAsync(request);

            if (!result.Success)
            {
                _logger.Warning("Error al actualizar prenda {CLPrendaCod}: {ErrorMessage}",
                    request?.CLPrendaCod, result.Error?.Message);

                var statusCode = result.Error?.HttpStatusCode ?? StatusCodes.Status400BadRequest;
                return StatusCode(statusCode, result);
            }

            _logger.Information("Prenda {CLPrendaCod} actualizada correctamente",
                request?.CLPrendaCod);
            return Ok(result);
        }
    }
}