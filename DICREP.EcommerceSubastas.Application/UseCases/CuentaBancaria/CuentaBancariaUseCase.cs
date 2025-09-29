// UseCases/CuentaBancaria/CuentaBancariaUseCase.cs
using DICREP.EcommerceSubastas.Application.DTOs.CuentaBancaria;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.Exceptions;
using DICREP.EcommerceSubastas.Application.Interfaces;
using Serilog;

namespace DICREP.EcommerceSubastas.Application.UseCases.CuentaBancaria
{
    public class CuentaBancariaUseCase
    {
        private readonly ICuentaBancariaRepository _cuentaBancariaRepository;
        private readonly ILogger _logger;

        public CuentaBancariaUseCase(ICuentaBancariaRepository cuentaBancariaRepository)
        {
            _cuentaBancariaRepository = cuentaBancariaRepository;
            _logger = Log.ForContext<CuentaBancariaUseCase>();
        }

        public async Task<ResponseDTO<CuentaBancariaResponseDTO>> ExecuteAsync(CuentaBancariaRequestDTO request)
        {
            if (request == null)
            {
                _logger.Warning("CuentaBancariaUseCase: request nulo");
                throw new DatosFaltantesException("Datos de la solicitud son requeridos");
            }

            _logger.Information("CuentaBancariaUseCase: procesando cuenta para organismo {OrganismoId}",
                request.OrganismoId);

            try
            {
                var result = await _cuentaBancariaRepository.GetOrCreateCuentaAsync(request);

                if (result.Success)
                {
                    _logger.Information("CuentaBancariaUseCase: cuenta procesada exitosamente. ID: {CuentaId}",
                        result.Data.CuentaId);
                }
                else
                {
                    _logger.Warning("CuentaBancariaUseCase: error al procesar cuenta - {ErrorMessage}",
                        result.Error?.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "CuentaBancariaUseCase: excepción inesperada");
                throw;
            }
        }
    }
}