using DICREP.EcommerceSubastas.Application.DTOs.FichaProducto;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.Exceptions;
using DICREP.EcommerceSubastas.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace DICREP.EcommerceSubastas.Application.UseCases.FichaProducto
{
    public class NotifyAuctionResultUseCase
    {
        private readonly ILogger _logger;
        private readonly IClAuctionApiService _clAuctionApiService;

        public NotifyAuctionResultUseCase(
            IClAuctionApiService clAuctionApiService)
        {
            _logger = Log.ForContext<NotifyAuctionResultUseCase>();
            _clAuctionApiService = clAuctionApiService;
        }

        public async Task<ResponseDTO<int>> ExecuteAsync(NotifyAuctionResultDto dto)
        {
            if (dto == null)
            {
                _logger.Warning("NotifyAuctionResultUseCase: DTO nulo");
                throw new DatosFaltantesException(nameof(dto));
            }

            _logger.Information("NotifyAuctionResult: procesando notificación a CL, producto ID {ProductId}",
                dto.id_publicacion_bien);

            try
            {
                // Llamar al servicio de CL API
                var apiResult = await _clAuctionApiService.UpdateProductStatusAsync(
                    dto.id_publicacion_bien,
                    dto.IdEstado // Asumo que tu Dto tiene esta propiedad
                );

                if (apiResult.Success)
                {
                    _logger.Information("CL API actualizó exitosamente producto {ProductId}: {Message}",
                        dto.id_publicacion_bien, apiResult.Message);

                    return new ResponseDTO<int>
                    {
                        Success = true,
                        Data = 1, // O algún ID relevante
                        Message = apiResult.Message
                    };
                }
                else
                {
                    _logger.Warning("Error en CL API para producto {ProductId}: {Error}",
                        dto.id_publicacion_bien, apiResult.Error);

                    // Manejar específicamente el error de foreign key
                    if (apiResult.Error?.Contains("foreign key constraint fails") == true)
                    {
                        return new ResponseDTO<int>
                        {
                            Success = false,
                            Message = "Error: " + apiResult.Error,
                            Data = 0,
                            Error = new ErrorResponseDto
                            {
                                Message = "El producto no existe en el sistema de CL",
                                HttpStatusCode = StatusCodes.Status404NotFound,
                                ErrorCode = 404,
                            }
                        };
                    }

                    return new ResponseDTO<int>
                    {
                        Success = false,
                        Message = "Error: " + apiResult.Error,
                        Data = 0,
                        Error = new ErrorResponseDto
                        {
                            Message = "Error al actualizar en CL",
                            HttpStatusCode = StatusCodes.Status502BadGateway,
                            ErrorCode = 502,
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Excepción inesperada en NotifyAuctionResultUseCase para producto ID {ProductId}",
                    dto.id_publicacion_bien);

                return new ResponseDTO<int>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = 0,
                    Error = new ErrorResponseDto
                    {
                        Message = "Error interno del servidor",
                        HttpStatusCode = StatusCodes.Status500InternalServerError,
                        ErrorCode = 500,
                    }
                };
            }
        }
    }
}
