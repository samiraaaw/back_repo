using DICREP.EcommerceSubastas.Application.DTOs.FichaProducto;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net;
using System.Text.Json;
using Serilog;

namespace DICREP.EcommerceSubastas.Infrastructure.Data.Repositories
{
    public class FichaProductoRepository : IFichaProductoRepository
    {
        private readonly EcoCircularContext _context;
        private readonly ILogger _logger;

        public FichaProductoRepository(EcoCircularContext context)
        {
            _context = context;
            _logger = Log.ForContext<FichaProductoRepository>(); // ✅ Serilog style
        }

        private static (string Message, int? HttpStatusCode, int? BusinessCode) MapSqlError(int errorCode, string originalMessage)
        {
            return errorCode switch
            {
                916 => ("Error de permisos de base de datos. El usuario no tiene acceso requerido.", 500, 1002),
                4060 => ("Error de conexión con la base de datos. No se puede acceder a la base de datos solicitada.", 503, 1001),
                18456 => ("Error de autenticación. Credenciales inválidas.", 500, 1003),
                50001 => (originalMessage, StatusCodes.Status400BadRequest, 40001),
                50002 => (originalMessage, StatusCodes.Status400BadRequest, 40002),
                50003 => (originalMessage, StatusCodes.Status400BadRequest, 40003),
                50004 => (originalMessage, StatusCodes.Status400BadRequest, 40004),
                50010 => (originalMessage, StatusCodes.Status400BadRequest, 40010),
                50011 => ("El id de la categoría recibido ya existe, pero con un nombre distinto", StatusCodes.Status400BadRequest, 40011),
                50012 => (originalMessage, StatusCodes.Status400BadRequest, 40012),
                50013 => (originalMessage, StatusCodes.Status400BadRequest, 40013),
                50014 => (originalMessage, StatusCodes.Status400BadRequest, 40014),
                _ => (originalMessage, StatusCodes.Status400BadRequest, 40015)
            };
        }


        public async Task<ResponseDTO<int>> SendFichaAsync(ReceiveFichaDto dto)
        {
            _logger.Information("Recibiendo ficha de producto con ID {ProductId}", dto.ficha_producto.detalle_bien.id_publicacion_bien);
            var response = new ResponseDTO<int>();
            try
            {
                var json = JsonSerializer.Serialize(dto);
                var param = new SqlParameter("@json", SqlDbType.NVarChar) { Value = json };

                int result = await _context.Database.ExecuteSqlRawAsync("EXEC sp_RecibirProductoJSON @json", param);
                _logger.Information("Ficha recibida correctamente para ID {ProductId}", dto.ficha_producto.detalle_bien.id_publicacion_bien);

                response.Data = 0;
                response.Success = true;
                response.Message = "Se ha recibido correctamente la ficha";
            }
            catch (SqlException ex)
            {
                if (ex.Class >= 20) // Otros errores de conexión graves
                {
                    _logger.Error(ex, "Error de conexión SQL: {SqlErrorNumber} - {SqlErrorMessage}", ex.Number, ex.Message);
                    response.Error = new ErrorResponseDto
                    {
                        ErrorCode = ex.Number,
                        Message = "Ha ocurrido un error interno al procesar la solicitud",
                        HttpStatusCode = 500
                    };
                    response.Data = 0;
                    response.Success = false;
                    response.Message = "Ha ocurrido un error interno al procesar la solicitud";
                }
                else
                {
                    // Error dentro del procedimiento almacenado
                    _logger.Error(ex, "Error en el procedimiento almacenado: {SqlErrorNumber} - {SqlErrorMessage}", ex.Number, ex.Message);
                    var (message, httpStatus, businessCode) = MapSqlError(ex.Number, ex.Message);

                    response.Error = new ErrorResponseDto
                    {
                        ErrorCode = businessCode ?? ex.Number,
                        Message = message ?? "Se produjo un error interno al procesar la solicitud.",
                        HttpStatusCode = httpStatus
                    };
                    response.Data = 0;
                    response.Success = false;
                    response.Message = "Ha ocurrido un error al recibir la ficha del producto";
                }
            }

            catch (Exception ex)
            {
                _logger.Error(ex, "Error inesperado al recibir ficha: {ExceptionMessage}", ex.Message);

                response.Error = new ErrorResponseDto
                {
                    ErrorCode = 500,
                    Message = "Se produjo un error interno al procesar la solicitud",
                    HttpStatusCode = 500
                };
                response.Data = 0;
                response.Success = false;
                response.Message = "Se produjo un error interno al procesar la solicitud";
            }
            return response;
        }
    }
}