// Data/Repositories/CuentaBancariaRepository.cs
using DICREP.EcommerceSubastas.Application.DTOs.CuentaBancaria;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;

namespace DICREP.EcommerceSubastas.Infrastructure.Data.Repositories
{
    public class CuentaBancariaRepository : ICuentaBancariaRepository
    {
        private readonly EcoCircularContext _context;
        private readonly ILogger _logger;

        public CuentaBancariaRepository(EcoCircularContext context)
        {
            _context = context;
            _logger = Log.ForContext<CuentaBancariaRepository>();
        }

        public async Task<ResponseDTO<CuentaBancariaResponseDTO>> GetOrCreateCuentaAsync(CuentaBancariaRequestDTO request)
        {
            _logger.Information("Procesando cuenta bancaria para organismo {OrganismoId}, cuenta {NumeroCuenta}",
                request.OrganismoId, request.NumeroCuenta);

            var response = new ResponseDTO<CuentaBancariaResponseDTO>();

            try
            {
                var cuentaIdParam = new SqlParameter("@Cuenta_ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var organismoIdParam = new SqlParameter("@Organismo_ID", request.OrganismoId);
                var bancoIdParam = new SqlParameter("@Banco_ID", request.BancoId);
                var tipoCuentaIdParam = new SqlParameter("@TipoCuenta_ID", request.TipoCuentaId);
                var numeroCuentaParam = new SqlParameter("@NumeroCuenta", request.NumeroCuenta ?? (object)DBNull.Value);
                var nombreCuentaParam = new SqlParameter("@NombreCuenta", request.NombreCuenta ?? (object)DBNull.Value);
                var rutParam = new SqlParameter("@Rut", request.Rut ?? (object)DBNull.Value);
                var correoParam = new SqlParameter("@Correo", request.Correo ?? (object)DBNull.Value);
                var usuarioParam = new SqlParameter("@Usuario", request.Usuario ?? (object)DBNull.Value);
                var origenParam = new SqlParameter("@Origen", request.Origen ?? "API");

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.sp_CuentaBancaria_GetOrCreate @Cuenta_ID OUTPUT, @Organismo_ID, @Banco_ID, @TipoCuenta_ID, @NumeroCuenta, @NombreCuenta, @Rut, @Correo, @Usuario, @Origen",
                    cuentaIdParam, organismoIdParam, bancoIdParam, tipoCuentaIdParam,
                    numeroCuentaParam, nombreCuentaParam, rutParam, correoParam, usuarioParam, origenParam);

                var cuentaId = (int)cuentaIdParam.Value;

                _logger.Information("Cuenta procesada exitosamente. ID: {CuentaId}", cuentaId);

                response.Success = true;
                response.Data = new CuentaBancariaResponseDTO
                {
                    CuentaId = cuentaId,
                    Mensaje = "Cuenta procesada exitosamente",
                    EsNueva = true // Se podría mejorar detectando si era existente
                };
                response.Message = "Operación exitosa";

                return response;
            }
            catch (SqlException ex)
            {
                _logger.Error(ex, "Error SQL al procesar cuenta bancaria: {ErrorNumber} - {ErrorMessage}",
                    ex.Number, ex.Message);

                var (message, httpStatus, businessCode) = MapSqlError(ex.Number, ex.Message);

                response.Success = false;
                response.Error = new ErrorResponseDto
                {
                    ErrorCode = businessCode ?? ex.Number,
                    Message = message,
                    HttpStatusCode = httpStatus ?? 400
                };
                response.Message = "Error al procesar cuenta bancaria";

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inesperado al procesar cuenta bancaria");

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
                61013 => ("El organismo especificado no existe", 400, 61013),
                61014 => ("El banco especificado no existe", 400, 61014),
                61015 => ("El tipo de cuenta especificado no existe", 400, 61015),
                2601 => ("Ya existe una cuenta con los mismos datos", 409, 2601),
                _ => (originalMessage, 400, errorCode)
            };
        }
    }
}