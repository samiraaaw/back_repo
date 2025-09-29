using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.DTOs.Subasta;
using DICREP.EcommerceSubastas.Application.Exceptions;
using DICREP.EcommerceSubastas.Application.Interfaces;
using Serilog;

namespace DICREP.EcommerceSubastas.Application.UseCases.Subasta
{
    public class ExtraccionSubastaUseCase
    {
        private readonly ISubastaRepository _subastaRepository;
        private readonly Serilog.ILogger _logger;

        public ExtraccionSubastaUseCase(ISubastaRepository subastaRepository)
        {
            _subastaRepository = subastaRepository;
            _logger = Log.ForContext<ExtraccionSubastaUseCase>();
        }

        public async Task<ResponseDTO<ExtraccionSubastaResponseDTO>> ExecuteAsync(ExtraccionSubastaRequestDTO request)
        {
            if (request == null)
            {
                _logger.Warning("ExtraccionSubastaUseCase: request nulo");
                throw new DatosFaltantesException("Datos de la solicitud son requeridos");
            }

            // Validación de fechas
            if (request.FechaDesde.HasValue && request.FechaHasta.HasValue &&
                request.FechaDesde > request.FechaHasta)
            {
                _logger.Warning("ExtraccionSubastaUseCase: rango de fechas inválido");
                throw new ReglaNegocioException("La fecha desde no puede ser mayor que la fecha hasta");
            }

            _logger.Information("ExtraccionSubastaUseCase: iniciando extracción para empleado {EmpId}", request.EmpId);

            try
            {
                var result = await _subastaRepository.ExtraccionSubastaAsync(request);

                if (result.Success)
                {
                    _logger.Information("ExtraccionSubastaUseCase: extracción completada. Total: {Total}",
                        result.Data?.TotalExtraidas);

                    if (result.Data?.TieneErrores == true)
                    {
                        _logger.Warning("ExtraccionSubastaUseCase: se detectaron {Count} errores de validación",
                            result.Data.ErroresValidacion.Count);
                    }
                }
                else
                {
                    _logger.Warning("ExtraccionSubastaUseCase: error en extracción - {ErrorMessage}",
                        result.Error?.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ExtraccionSubastaUseCase: excepción inesperada");
                throw;
            }
        }
    }
}
