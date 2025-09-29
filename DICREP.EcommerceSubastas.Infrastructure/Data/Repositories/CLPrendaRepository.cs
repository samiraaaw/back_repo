// Data/Repositories/CLPrendaRepository.cs
using DICREP.EcommerceSubastas.Application.DTOs.CLPrenda;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;

namespace DICREP.EcommerceSubastas.Infrastructure.Data.Repositories
{
    public class CLPrendaRepository : ICLPrendaRepository
    {
        private readonly EcoCircularContext _context;
        private readonly ILogger _logger;

        public CLPrendaRepository(EcoCircularContext context)
        {
            _context = context;
            _logger = Log.ForContext<CLPrendaRepository>();
        }

        public async Task<ResponseDTO<CLPrendaUpdateResponseDTO>> UpdateIncrementoComisionAsync(CLPrendaUpdateRequestDTO request)
        {
            _logger.Information("Actualizando prenda {CLPrendaCod} - Incremento: {Incremento}, Comisión: {Comision}",
                request.CLPrendaCod, request.ValorIncremento, request.ValorComision);

            var response = new ResponseDTO<CLPrendaUpdateResponseDTO>();

            try
            {
                var clPrendaParam = new SqlParameter("@CLPrenda_Cod", request.CLPrendaCod);
                var valorIncrementoParam = new SqlParameter("@ValorIncremento", request.ValorIncremento ?? (object)DBNull.Value);
                var empIdParam = new SqlParameter("@Emp_ID", request.EmpId);
                var pcParam = new SqlParameter("@PC", request.PC ?? (object)DBNull.Value);
                var valorComisionParam = new SqlParameter("@ValorComision", request.ValorComision ?? (object)DBNull.Value);

                // Ejecutar el procedimiento almacenado que retorna la fila actualizado
                var results = await _context.Database.SqlQueryRaw<CLPrendaResult>(
                "EXEC dbo.sp_CLPrenda_IncPorc_SetByCod @CLPrenda_Cod, @ValorIncremento, @Emp_ID, @PC, @ValorComision",
                clPrendaParam, valorIncrementoParam, empIdParam, pcParam, valorComisionParam)
                .ToListAsync();

                var result = results.FirstOrDefault();


                if (result != null)
                {
                    _logger.Information("Prenda {CLPrendaCod} actualizada exitosamente. ID: {CLPrendaId}",
                        request.CLPrendaCod, result.CLPrenda_ID);

                    response.Success = true;
                    response.Data = new CLPrendaUpdateResponseDTO
                    {
                        CLPrendaId = result.CLPrenda_ID,
                        CLPrendaCod = result.CLPrenda_Cod,
                        CLPrendaNombre = result.CLPrenda_Nombre,
                        CLPrendaIncremento = result.CLPrenda_Incremento,
                        CLPrendaComision = result.CLPrenda_Comision,
                        Mensaje = "Prenda actualizada exitosamente"
                    };
                    response.Message = "Operación exitosa";
                }
                else
                {
                    _logger.Warning("No se retornó información de la prenda actualizada");
                    response.Success = false;
                    response.Message = "Error: no se pudo obtener información de la prenda actualizada";
                }

                return response;
            }
            catch (SqlException ex)
            {
                _logger.Error(ex, "Error SQL al actualizar prenda {CLPrendaCod}: {ErrorNumber} - {ErrorMessage}",
                    request.CLPrendaCod, ex.Number, ex.Message);

                var (message, httpStatus, businessCode) = MapSqlError(ex.Number, ex.Message);

                response.Success = false;
                response.Error = new ErrorResponseDto
                {
                    ErrorCode = businessCode ?? ex.Number,
                    Message = message,
                    HttpStatusCode = httpStatus ?? 400
                };
                response.Message = "Error al actualizar prenda";

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inesperado al actualizar prenda {CLPrendaCod}", request.CLPrendaCod);

                response.Success = false;
                response.Error = new ErrorResponseDto
                {
                    ErrorCode = 500,
                    Message = "Error interno del servidor",
                    HttpStatusCode = 500
                };
                response.Message = "Error interno del servidor";

                return response;
            }
        }

        private static (string Message, int? HttpStatusCode, int? BusinessCode) MapSqlError(int errorCode, string originalMessage)
        {
            return errorCode switch
            {
                53001 => ("Debe proporcionar el código de la prenda", 400, 53001),
                53003 => ("ID de empleado requerido para auditoría", 400, 53003),
                53004 => ("No existe una prenda con el código indicado", 404, 53004),
                53005 => ("No se detectaron cambios para actualizar", 400, 53005),
                53006 => ("Valor de comisión inválido", 400, 53006),
                _ => (originalMessage, 400, errorCode)
            };
        }

        // Clase para mapear el resultado del procedimiento almacenado
        public class CLPrendaResult
        {
            public long CLPrenda_ID { get; set; }
            public string CLPrenda_Cod { get; set; }
            public string CLPrenda_Nombre { get; set; }
            public decimal? CLPrenda_Incremento { get; set; }
            public decimal? CLPrenda_Comision { get; set; }
        }
    }
}