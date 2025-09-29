// Controllers/CuentaBancariaController.cs
using DICREP.EcommerceSubastas.API.Filters;
using DICREP.EcommerceSubastas.Application.DTOs.CuentaBancaria;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.UseCases.CuentaBancaria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DICREP.EcommerceSubastas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] 
    public class CuentaBancariaController : ControllerBase
    {
        private readonly CuentaBancariaUseCase _cuentaBancariaUseCase;
        private readonly Serilog.ILogger _logger;

        public CuentaBancariaController(CuentaBancariaUseCase cuentaBancariaUseCase)
        {
            _cuentaBancariaUseCase = cuentaBancariaUseCase;
            _logger = Log.ForContext<CuentaBancariaController>();
        }

        /// <summary>
        /// Crea o obtiene una cuenta bancaria basada en coincidencia exacta
        /// </summary>
        /// <param name="request">Datos de la cuenta bancaria</param>
        /// <returns>Información de la cuenta procesada</returns>
        [HttpPost("GetOrCreate")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO<CuentaBancariaResponseDTO>>> GetOrCreateCuenta(
            [FromBody] CuentaBancariaRequestDTO request)
        {
            _logger.Information("Recibiendo solicitud de cuenta bancaria para organismo {OrganismoId}",
                request?.OrganismoId);

            var result = await _cuentaBancariaUseCase.ExecuteAsync(request);

            if (!result.Success)
            {
                _logger.Warning("Error al procesar cuenta bancaria: {ErrorMessage}",
                    result.Error?.Message);

                var statusCode = result.Error?.HttpStatusCode ?? StatusCodes.Status400BadRequest;
                return StatusCode(statusCode, result);
            }

            _logger.Information("Cuenta bancaria procesada correctamente. ID: {CuentaId}",
                result.Data?.CuentaId);
            return Ok(result);
        }
    }
}