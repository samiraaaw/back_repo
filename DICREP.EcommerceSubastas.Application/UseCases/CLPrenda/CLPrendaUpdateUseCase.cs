using DICREP.EcommerceSubastas.Application.DTOs.CLPrenda;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.Exceptions;
using DICREP.EcommerceSubastas.Application.Interfaces;
using Serilog;

namespace DICREP.EcommerceSubastas.Application.UseCases.CLPrenda
{
    public class CLPrendaUpdateUseCase
    {
        private readonly ICLPrendaRepository _clPrendaRepository;
        private readonly ILogger _logger;

        public CLPrendaUpdateUseCase(ICLPrendaRepository clPrendaRepository)
        {
            _clPrendaRepository = clPrendaRepository;
            _logger = Log.ForContext<CLPrendaUpdateUseCase>();
        }

        public async Task<ResponseDTO<CLPrendaUpdateResponseDTO>> ExecuteAsync(CLPrendaUpdateRequestDTO request)
        {
            if (request == null)
            {
                _logger.Warning("CLPrendaUpdateUseCase: request nulo");
                throw new DatosFaltantesException("Datos de la solicitud son requeridos");
            }

            // Validar que al menos uno de los valores esté presente
            if (!request.ValorIncremento.HasValue && !request.ValorComision.HasValue)
            {
                _logger.Warning("CLPrendaUpdateUseCase: no se proporcionó ningún valor para actualizar");
                throw new ReglaNegocioException("Debe proporcionar al menos un valor para actualizar (incremento o comisión)");
            }

            _logger.Information("CLPrendaUpdateUseCase: procesando actualización para prenda {CLPrendaCod}",
                request.CLPrendaCod);

            try
            {
                var result = await _clPrendaRepository.UpdateIncrementoComisionAsync(request);

                if (result.Success)
                {
                    _logger.Information("CLPrendaUpdateUseCase: prenda {CLPrendaCod} actualizada exitosamente",
                        request.CLPrendaCod);
                }
                else
                {
                    _logger.Warning("CLPrendaUpdateUseCase: error al actualizar prenda {CLPrendaCod} - {ErrorMessage}",
                        request.CLPrendaCod, result.Error?.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "CLPrendaUpdateUseCase: excepción inesperada para prenda {CLPrendaCod}",
                    request.CLPrendaCod);
                throw;
            }
        }
    }
}
