using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Domain.Entities;
using DICREP.EcommerceSubastas.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;

namespace DICREP.EcommerceSubastas.Infrastructure.Data.Repositories
{
    public class FeriadosRepository : IFeriadosRepository
    {

        private readonly EcoCircularContext _context;
        private readonly ILogger _logger;


        public FeriadosRepository(EcoCircularContext context)
        {
            _context = context;
            _logger = Log.ForContext<FeriadosRepository>(); // ✅ Serilog style
        }

        public async Task<bool> ExistsByFechaAsync(DateOnly fecha)
        {
            _logger.Information("Verificando existencia de feriado con fecha {Fecha}", fecha);
            try
            {
                var exists = await _context.Feriados.AnyAsync(f => f.Fecha == fecha);
                _logger.Information("Resultado verificación de feriado {Fecha}: {Existe}", fecha, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error al verificar existencia de feriado con fecha {Fecha}", fecha);
                throw;
            }
        }


        public async Task<ResponseDTO<int>> populateHolidaysTableBulk(List<Feriado> feriados)
        {
            _logger.Information("Iniciando inserción masiva de {Cantidad} feriados", feriados.Count);
            var response = new ResponseDTO<int>();
            try
            {
                _logger.Debug("Detalle de feriados a insertar: {@Feriados}", feriados.Take(5)); // Muestra primeros 5
                var feriadosTable = new DataTable();
                feriadosTable.Columns.Add("Fecha", typeof(DateTime));
                feriadosTable.Columns.Add("Descripcion", typeof(string));
                feriadosTable.Columns.Add("EsRegional", typeof(bool));
                feriadosTable.Columns.Add("Activo", typeof(bool));

                foreach (var entity in feriados)
                {
                    feriadosTable.Rows.Add(entity.Fecha.ToDateTime(TimeOnly.MinValue),
                                          entity.Descripcion,
                                          entity.EsRegional,
                                          entity.Activo);
                }

                var feriadosParam = new SqlParameter("@Feriados", SqlDbType.Structured)
                {
                    TypeName = "dbo.FeriadoTT",
                    Value = feriadosTable
                };

                var result = await _context.Database.ExecuteSqlRawAsync(
                   "EXEC sp_Feriado_UpsertBulk @Feriados, @SobrescribirDescripcion, @SobrescribirRegional, @ActivarSiNull",
                   feriadosParam,
                   new SqlParameter("@SobrescribirDescripcion", true),
                   new SqlParameter("@SobrescribirRegional", false),
                   new SqlParameter("@ActivarSiNull", true)
                );

                response.Data = 0;
                response.Success = true;
                response.Message = $"Se insertaron {feriados.Count} feriados con éxito";
                _logger.Information("Inserción masiva de feriados completada. Cantidad: {Cantidad}", feriados.Count);
            }
            catch (SqlException ex)
            {
                _logger.Error(ex, "Error de SQL al poblar tabla de feriados");
                response.Error = new ErrorResponseDto
                {
                    ErrorCode = 500,
                    Message = ex.Message,
                    HttpStatusCode = 500
                };
                response.Data = 0;
                response.Success = false;
                response.Message = "Ha ocurrido un error al poblar la tabla Feriados";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inesperado al poblar tabla de feriados");
                response.Error = new ErrorResponseDto
                {
                    ErrorCode = 500,
                    Message = ex.Message,
                    HttpStatusCode = 500
                };
                response.Data = 0;
                response.Success = false;
                response.Message = "Ha ocurrido un error al poblar la tabla Feriados";
            }
            return response;
        }

    }
}
