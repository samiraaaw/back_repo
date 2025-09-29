using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.Exceptions;
using DICREP.EcommerceSubastas.Application.Helpers;
using DICREP.EcommerceSubastas.Application.Interfaces;
using System.Text;
using DICREP.EcommerceSubastas.Application.DTOs.FichaProducto;
using Serilog;

namespace DICREP.EcommerceSubastas.Application.UseCases.FichaProducto
{
    public class ReceiveFichaUseCase
    {
        private readonly IFichaProductoRepository _ifichaProductoRepository;
        private readonly ILogger _logger;

        public ReceiveFichaUseCase(IFichaProductoRepository ifichaProductoRepository)
        {
            _ifichaProductoRepository = ifichaProductoRepository;
            _logger = Log.ForContext<ReceiveFichaUseCase>();
        }


        public async Task<ResponseDTO<int>> ExecuteAsync(ReceiveFichaDto requestDto)
        {
            var sbResultado = new StringBuilder();
            if (requestDto == null)
            {
                _logger.Warning("ReceiveFichaUseCase: requestDto nulo");
                throw new DatosFaltantesException(nameof(requestDto));
            }
            var ficha = requestDto.ficha_producto;
            _logger.Information("ReceiveFichaUseCase: procesando producto ID {ProductId}", ficha.detalle_bien.id_publicacion_bien);

            try
            {
                ficha.detalle_bien.nombre = NormalizacionHelper.NormalizarNombre(ficha.detalle_bien.nombre);
                ficha.detalle_bien.contacto_organismo.nombre = NormalizacionHelper.NormalizarNombre(ficha.detalle_bien.contacto_organismo.nombre);
                ficha.detalle_bien.categoria = NormalizacionHelper.NormalizarNombre(ficha.detalle_bien.categoria);
                ficha.detalle_bien.estado = NormalizacionHelper.NormalizarNombre(ficha.detalle_bien.estado);

                var response = await _ifichaProductoRepository.SendFichaAsync(requestDto);
                if (response.Success)
                {
                    _logger.Information("ReceiveFichaUseCase: ficha procesada correctamente para producto ID {ProductId}", ficha.detalle_bien.id_publicacion_bien);
                }
                else
                {
                    _logger.Error("ReceiveFichaUseCase: error al procesar ficha producto ID {ProductId} - {ErrorMessage}",
                        ficha.detalle_bien.id_publicacion_bien,
                        response.Error?.Message);
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ReceiveFichaUseCase: excepción inesperada al procesar producto ID {ProductId}", ficha.detalle_bien.id_publicacion_bien);
                throw;
            }
        }
    }
}