
// DICREP.EcommerceSubastas.Application/UseCases/Subasta/CargaResultadoSubastaUseCase.cs
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.DTOs.Subasta;
using DICREP.EcommerceSubastas.Application.Exceptions;
using DICREP.EcommerceSubastas.Application.Interfaces;
using Serilog;

namespace DICREP.EcommerceSubastas.Application.UseCases.Subasta
{
    public class CargaResultadoSubastaUseCase
    {
        private readonly ISubastaRepository _subastaRepository;
        private readonly Serilog.ILogger _logger;

        public CargaResultadoSubastaUseCase(ISubastaRepository subastaRepository)
        {
            _subastaRepository = subastaRepository;
            _logger = Log.ForContext<CargaResultadoSubastaUseCase>();
        }

        public async Task<ResponseDTO<CargaResultadoSubastaResponseDTO>> ExecuteAsync(CargaResultadoSubastaRequestDTO request)
        {
            if (request == null)
            {
                _logger.Warning("CargaResultadoSubastaUseCase: request nulo");
                throw new DatosFaltantesException("Datos de la solicitud son requeridos");
            }

            // Validación específica para estado "Rematado"
            if (string.Equals(request.EstadoTexto, "Rematado", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.AdjRut))
                {
                    _logger.Warning("CargaResultadoSubastaUseCase: RUT de adjudicatario requerido para estado Rematado");
                    throw new ReglaNegocioException("Para estado 'Rematado' debe proporcionar el RUT del adjudicatario");
                }
            }

            _logger.Information("CargaResultadoSubastaUseCase: procesando resultado para prenda {CLPrendaCod} con estado {Estado}",
                request.CLPrendaCod, request.EstadoTexto);


            try
            {

                var result = await _subastaRepository.CargarResultadoFilaAsync(request);

                if (result.Success)
                {
                    _logger.Information("CargaResultadoSubastaUseCase: resultado cargado exitosamente para prenda {CLPrendaCod}",
                        request.CLPrendaCod);
                }
                else
                {
                    _logger.Warning("CargaResultadoSubastaUseCase: error al cargar resultado para prenda {CLPrendaCod} - {ErrorMessage}",
                        request.CLPrendaCod, result.Error?.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "CargaResultadoSubastaUseCase: excepción inesperada para prenda {CLPrendaCod}",
                    request.CLPrendaCod);
                throw;
            }
        }

        public async Task<ResponseDTO<CargaResultadosLoteResponseDTO>> ExecuteLoteAsync(CargaResultadosLoteRequestDTO request)
        {
            if (request == null)
            {
                _logger.Warning("CargaResultadoSubastaUseCase: request de lote nulo");
                throw new DatosFaltantesException("Datos de la solicitud son requeridos");
            }

            if (request.Resultados == null || !request.Resultados.Any())
            {
                _logger.Warning("CargaResultadoSubastaUseCase: lista de resultados vacía");
                throw new ReglaNegocioException("Debe proporcionar al menos un resultado para procesar");
            }

            _logger.Information("CargaResultadoSubastaUseCase: procesando lote de {Count} resultados",
                request.Resultados.Count);

            // Validar duplicados por código de prenda
            var codigosDuplicados = request.Resultados
                .GroupBy(r => r.CLPrendaCod, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (codigosDuplicados.Any())
            {
                _logger.Warning("CargaResultadoSubastaUseCase: códigos duplicados detectados: {Duplicados}",
                    string.Join(", ", codigosDuplicados));
                throw new ReglaNegocioException($"Códigos de prenda duplicados: {string.Join(", ", codigosDuplicados)}");
            }

            try
            {
                var result = await _subastaRepository.CargarResultadosLoteAsync(request);

                if (result.Success)
                {
                    _logger.Information("CargaResultadoSubastaUseCase: lote procesado. Total: {Total}, Exitosos: {Exitosos}, Errores: {Errores}",
                        result.Data.TotalProcesados, result.Data.TotalExitosos, result.Data.TotalErrores);
                }
                else
                {
                    _logger.Warning("CargaResultadoSubastaUseCase: error al procesar lote - {ErrorMessage}",
                        result.Error?.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "CargaResultadoSubastaUseCase: excepción inesperada en procesamiento de lote");
                throw;
            }
        }
    }
}