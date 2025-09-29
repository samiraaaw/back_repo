using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Data/Repositories/SubastaRepository.cs
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.DTOs.Subasta;
using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;

namespace DICREP.EcommerceSubastas.Infrastructure.Data.Repositories
{
    public class SubastaRepository : ISubastaRepository
    {
        private readonly EcoCircularContext _context;
        private readonly Serilog.ILogger _logger;

        public SubastaRepository(EcoCircularContext context)
        {
            _context = context;
            _logger = Log.ForContext<SubastaRepository>();
        }





        // REEMPLAZA el método ExtraccionSubastaAsync en SubastaRepository.cs con esta versión corregida:
        public async Task<ResponseDTO<ExtraccionSubastaResponseDTO>> ExtraccionSubastaAsync(ExtraccionSubastaRequestDTO request)
        {
            _logger.Information("Iniciando extracción de subasta para empleado {EmpId}", request.EmpId);

            var response = new ResponseDTO<ExtraccionSubastaResponseDTO>();

            try
            {
                // Preparar parámetros
                var totalExtraidasParam = new SqlParameter("@TotalExtraidas", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                var fechaDesdeParam = new SqlParameter("@FechaDesde", request.FechaDesde ?? (object)DBNull.Value);
                var fechaHastaParam = new SqlParameter("@FechaHasta", request.FechaHasta ?? (object)DBNull.Value);
                var organismoIdParam = new SqlParameter("@Organismo_ID", request.OrganismoId ?? (object)DBNull.Value);
                var estBienIdParam = new SqlParameter("@EstBien_ID", request.EstBienId ?? (object)DBNull.Value);
                var empIdParam = new SqlParameter("@Emp_ID", request.EmpId);
                var pcParam = new SqlParameter("@PC", request.PC ?? (object)DBNull.Value);
                var comisionParam = new SqlParameter("@Comision", request.Comision ?? (object)DBNull.Value);
                var incrementoParam = new SqlParameter("@Incremento", request.Incremento ?? (object)DBNull.Value);

                // Inicializar el resultado ANTES de usar el DataReader
                var result = new ExtraccionSubastaResponseDTO
                {
                    TotalExtraidas = 0,
                    ErroresValidacion = new List<PrendaValidacionErrorDTO>(),
                    PrendasExtraidas = new List<ExtraccionSubastaItemDTO>(),
                    Mensaje = "Iniciando extracción..."
                };

                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "dbo.sp_Extraccion_Subasta";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(fechaDesdeParam);
                command.Parameters.Add(fechaHastaParam);
                command.Parameters.Add(organismoIdParam);
                command.Parameters.Add(estBienIdParam);
                command.Parameters.Add(empIdParam);
                command.Parameters.Add(pcParam);
                command.Parameters.Add(comisionParam);
                command.Parameters.Add(incrementoParam);
                command.Parameters.Add(totalExtraidasParam);

                // Asegurar que la conexión esté abierta
                if (_context.Database.GetDbConnection().State != ConnectionState.Open)
                {
                    await _context.Database.OpenConnectionAsync();
                }

                using var reader = await command.ExecuteReaderAsync();

                // Primer resultset: Errores de validación (si existe)
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        // Verificar que las columnas existen antes de leerlas
                        var error = new PrendaValidacionErrorDTO();

                        if (HasColumn(reader, "CLPrenda_ID"))
                            error.CLPrendaId = reader.GetInt64("CLPrenda_ID");

                        if (HasColumn(reader, "CLPrenda_Cod"))
                            error.CLPrendaCod = reader.GetString("CLPrenda_Cod");

                        if (HasColumn(reader, "Motivo"))
                            error.Motivo = reader.GetString("Motivo");

                        result.ErroresValidacion.Add(error);
                    }
                }

                // Si hay errores, el procedimiento podría lanzar excepción
                // Si no hay errores, leer el segundo resultset con las prendas
                if (await reader.NextResultAsync() && reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        var prenda = new ExtraccionSubastaItemDTO();

                        // Mapear solo las columnas que existen, usando métodos seguros
                        prenda.CLPrendaCodigo = GetSafeString(reader, "CLPrenda_Codigo");
                        prenda.FechaIngresoFase2 = GetSafeDateTime(reader, "Fecha ingreso Fase 2");
                        prenda.FechaTerminoFase2 = GetSafeDateTime(reader, "Fecha termino Fase 2");
                        prenda.Categoria = GetSafeString(reader, "Categoria");
                        prenda.NombreProducto = GetSafeString(reader, "NombreProducto");
                        prenda.Descripcion = GetSafeString(reader, "Descripción");
                        prenda.EstadoBien = GetSafeString(reader, "Estado bien");
                        prenda.Cantidad = GetSafeInt(reader, "Cantidad");
                        prenda.PrecioTotal = GetSafeDecimal(reader, "Precio Total (Unidad x Cantidad)");
                        prenda.Comision = GetSafeDecimal(reader, "Comisión");
                        prenda.Incremento = GetSafeDecimal(reader, "Incremento");

                        // Datos del organismo
                        prenda.NombreOrganizacion = GetSafeString(reader, "Nombre Organización");
                        prenda.RutOrganizacion = GetSafeString(reader, "Rut Organización");
                        prenda.Correo = GetSafeString(reader, "Correo");
                        prenda.Telefono = GetSafeString(reader, "Telefono");
                        prenda.Comuna = GetSafeString(reader, "Comuna");
                        prenda.DireccionContacto = GetSafeString(reader, "Direccion contacto");
                        prenda.Region = GetSafeString(reader, "Región");

                        // Fotos
                        prenda.Foto1 = GetSafeString(reader, "Foto1");
                        prenda.Foto2 = GetSafeString(reader, "Foto2");
                        prenda.Foto3 = GetSafeString(reader, "Foto3");
                        prenda.Foto4 = GetSafeString(reader, "Foto4");
                        prenda.Foto5 = GetSafeString(reader, "Foto5");
                        prenda.Foto6 = GetSafeString(reader, "Foto6");

                        // Informes
                        prenda.Informe1 = GetSafeString(reader, "Informe1");
                        prenda.Informe2 = GetSafeString(reader, "Informe2");
                        prenda.Informe3 = GetSafeString(reader, "Informe3");
                        prenda.Informe4 = GetSafeString(reader, "Informe4");
                        prenda.Informe5 = GetSafeString(reader, "Informe5");
                        prenda.Informe6 = GetSafeString(reader, "Informe6");

                        result.PrendasExtraidas.Add(prenda);
                    }
                }

                // Obtener el total de extraídas del parámetro OUTPUT
                if (totalExtraidasParam.Value != DBNull.Value)
                {
                    result.TotalExtraidas = Convert.ToInt32(totalExtraidasParam.Value);
                }

                result.Mensaje = result.TotalExtraidas == 0
                    ? "No se encontraron prendas que cumplan los criterios de extracción"
                    : $"Se extrajeron {result.TotalExtraidas} prendas exitosamente";

                _logger.Information("Extracción completada. Total extraídas: {Total}", result.TotalExtraidas);

                response.Success = true;
                response.Data = result;
                response.Message = "Extracción realizada exitosamente";

                return response;
            }
            catch (SqlException ex)
            {
                _logger.Error(ex, "Error SQL en extracción de subasta: {ErrorNumber} - {ErrorMessage}",
                    ex.Number, ex.Message);

                var (message, httpStatus, businessCode) = MapSqlError(ex.Number, ex.Message);

                response.Success = false;
                response.Error = new ErrorResponseDto
                {
                    ErrorCode = businessCode ?? ex.Number,
                    Message = message,
                    HttpStatusCode = httpStatus ?? 400
                };
                response.Message = "Error en la extracción de subasta";

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inesperado en extracción de subasta: {Message}", ex.Message);

                response.Success = false;
                response.Error = new ErrorResponseDto
                {
                    ErrorCode = 500,
                    Message = $"Error interno del servidor: {ex.Message}",
                    HttpStatusCode = 500
                };
                response.Message = "Error interno del servidor";

                return response;
            }
            finally
            {
                if (_context.Database.GetDbConnection().State == ConnectionState.Open)
                {
                    await _context.Database.CloseConnectionAsync();
                }
            }
        }

        // Métodos auxiliares para lectura segura de datos
        private static bool HasColumn(System.Data.Common.DbDataReader reader, string columnName)
        {
            try
            {
                return reader.GetOrdinal(columnName) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private static string GetSafeString(System.Data.Common.DbDataReader reader, string columnName)
        {
            try
            {
                if (HasColumn(reader, columnName) && !reader.IsDBNull(columnName))
                    return reader.GetString(columnName);
            }
            catch { }
            return null;
        }

        private static DateTime? GetSafeDateTime(System.Data.Common.DbDataReader reader, string columnName)
        {
            try
            {
                if (HasColumn(reader, columnName) && !reader.IsDBNull(columnName))
                    return reader.GetDateTime(columnName);
            }
            catch { }
            return null;
        }

        private static int? GetSafeInt(System.Data.Common.DbDataReader reader, string columnName)
        {
            try
            {
                if (HasColumn(reader, columnName) && !reader.IsDBNull(columnName))
                    return reader.GetInt32(columnName);
            }
            catch { }
            return null;
        }

        private static decimal? GetSafeDecimal(System.Data.Common.DbDataReader reader, string columnName)
        {
            try
            {
                if (HasColumn(reader, columnName) && !reader.IsDBNull(columnName))
                    return reader.GetDecimal(columnName);
            }
            catch { }
            return null;
        }



        private static (string Message, int? HttpStatusCode, int? BusinessCode) MapSqlError(int errorCode, string originalMessage)
        {
            return errorCode switch
            {
                53003 => ("ID de empleado requerido para auditoría", 400, 53003),
                53006 => ("Rango de fechas inválido: FechaDesde > FechaHasta", 400, 53006),
                53011 => (originalMessage, 400, 53011), // Prendas con campos inválidos
                _ => (originalMessage, 400, errorCode)
            };
        }


        // Reemplaza el método CargarResultadoFilaAsync en SubastaRepository.cs:

        public async Task<ResponseDTO<CargaResultadoSubastaResponseDTO>> CargarResultadoFilaAsync(CargaResultadoSubastaRequestDTO request)
        {
            _logger.Information("Iniciando carga de resultado para prenda {CLPrendaCod} con estado {Estado}",
                request.CLPrendaCod, request.EstadoTexto);

            var response = new ResponseDTO<CargaResultadoSubastaResponseDTO>();

            // USAR TRANSACCIÓN EXPLÍCITA DE ENTITY FRAMEWORK
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Preparar parámetros para el stored procedure
                var clPrendaCodParam = new SqlParameter("@CLPrenda_Cod", request.CLPrendaCod);
                var clPrendaSubastaParam = new SqlParameter("@CLPrenda_Subasta", request.CLPrendaSubasta ?? (object)DBNull.Value);
                var fechaSubParam = new SqlParameter("@FechaSub", request.FechaSub ?? (object)DBNull.Value);
                var estadoTextoParam = new SqlParameter("@EstadoTexto", request.EstadoTexto);

                var montoMinimoParam = new SqlParameter("@MontoMinimo", request.MontoMinimo ?? (object)DBNull.Value);
                var montoAdjudicacionParam = new SqlParameter("@MontoAdjudicacion", request.MontoAdjudicacion ?? (object)DBNull.Value);
                var totalAdjudicacionParam = new SqlParameter("@TotalAdjudicacion", request.TotalAdjudicacion ?? (object)DBNull.Value);
                var fechaAdjudicacionParam = new SqlParameter("@FechaAdjudicacion", request.FechaAdjudicacion ?? (object)DBNull.Value);
                var monedaParam = new SqlParameter("@Moneda", request.Moneda ?? (object)DBNull.Value);
                var comisionPorcParam = new SqlParameter("@ComisionPorc", request.ComisionPorc ?? (object)DBNull.Value);
                var totalComisionParam = new SqlParameter("@TotalComision", request.TotalComision ?? (object)DBNull.Value);
                var ivaComisionParam = new SqlParameter("@IvaComision", request.IvaComision ?? (object)DBNull.Value);
                var totalRecaudarParam = new SqlParameter("@TotalRecaudar", request.TotalRecaudar ?? (object)DBNull.Value);

                var adjRutParam = new SqlParameter("@Adj_Rut", request.AdjRut ?? (object)DBNull.Value);
                var adjNombreParam = new SqlParameter("@Adj_Nombre", request.AdjNombre ?? (object)DBNull.Value);
                var adjNombreSParam = new SqlParameter("@Adj_NombreS", request.AdjNombreS ?? (object)DBNull.Value);
                var adjApellidoPParam = new SqlParameter("@Adj_ApellidoP", request.AdjApellidoP ?? (object)DBNull.Value);
                var adjApellidoMParam = new SqlParameter("@Adj_ApellidoM", request.AdjApellidoM ?? (object)DBNull.Value);
                var adjCorreoParam = new SqlParameter("@Adj_Correo", request.AdjCorreo ?? (object)DBNull.Value);
                var adjComunaNombreParam = new SqlParameter("@Adj_ComunaNombre", request.AdjComunaNombre ?? (object)DBNull.Value);

                var empIdParam = new SqlParameter("@Emp_ID", request.EmpId);
                var pcParam = new SqlParameter("@PC", request.PC ?? (object)DBNull.Value);

                // Ejecutar el stored procedure dentro de la transacción de EF
                await _context.Database.ExecuteSqlRawAsync(
                    @"EXEC dbo.sp_Subasta_CargarResultado_Fila 
                @CLPrenda_Cod, @CLPrenda_Subasta, @FechaSub, @EstadoTexto,
                @MontoMinimo, @MontoAdjudicacion, @TotalAdjudicacion, @FechaAdjudicacion,
                @Moneda, @ComisionPorc, @TotalComision, @IvaComision, @TotalRecaudar,
                @Adj_Rut, @Adj_Nombre, @Adj_NombreS, @Adj_ApellidoP, @Adj_ApellidoM, 
                @Adj_Correo, @Adj_ComunaNombre, @Emp_ID, @PC",
                    clPrendaCodParam, clPrendaSubastaParam, fechaSubParam, estadoTextoParam,
                    montoMinimoParam, montoAdjudicacionParam, totalAdjudicacionParam, fechaAdjudicacionParam,
                    monedaParam, comisionPorcParam, totalComisionParam, ivaComisionParam, totalRecaudarParam,
                    adjRutParam, adjNombreParam, adjNombreSParam, adjApellidoPParam, adjApellidoMParam,
                    adjCorreoParam, adjComunaNombreParam, empIdParam, pcParam);

                // Confirmar la transacción
                await transaction.CommitAsync();

                _logger.Information("Resultado cargado exitosamente para prenda {CLPrendaCod}", request.CLPrendaCod);

                // Mapear estado texto a descripción
                var estadoDescripcion = request.EstadoTexto.ToUpper() switch
                {
                    "REMATADO" => "Rematado",
                    "SIN POSTOR" => "Sin postor",
                    "NO PAGADO" => "No pagado",
                    _ => request.EstadoTexto
                };

                response.Success = true;
                response.Data = new CargaResultadoSubastaResponseDTO
                {
                    CLPrendaCod = request.CLPrendaCod,
                    EstadoAnterior = "En subasta",
                    EstadoNuevo = estadoDescripcion,
                    TieneAdjudicatario = request.EstadoTexto.Equals("Rematado", StringComparison.OrdinalIgnoreCase),
                    Mensaje = "Resultado cargado exitosamente",
                    FechaProcesamiento = DateTime.Now
                };
                response.Message = "Resultado cargado exitosamente";

                return response;
            }
            catch (SqlException ex)
            {
                _logger.Error(ex, "Error SQL al cargar resultado para prenda {CLPrendaCod}: {ErrorNumber} - {ErrorMessage}",
                    request.CLPrendaCod, ex.Number, ex.Message);

                // Rollback automático al salir del using, pero podemos hacerlo explícito
                await transaction.RollbackAsync();

                var (message, httpStatus, businessCode) = MapSqlErrorCargaResultado(ex.Number, ex.Message);

                response.Success = false;
                response.Error = new ErrorResponseDto
                {
                    ErrorCode = businessCode ?? ex.Number,
                    Message = message,
                    HttpStatusCode = httpStatus ?? 400
                };
                response.Message = "Error al cargar resultado de subasta";

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inesperado al cargar resultado para prenda {CLPrendaCod}", request.CLPrendaCod);

                await transaction.RollbackAsync();

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


        // VERSIÓN FINAL del método CargarResultadosLoteAsync en SubastaRepository.cs:

        public async Task<ResponseDTO<CargaResultadosLoteResponseDTO>> CargarResultadosLoteAsync(CargaResultadosLoteRequestDTO request)
        {
            _logger.Information("Iniciando carga secuencial de lote de {Count} resultados de subasta", request.Resultados.Count);

            var response = new ResponseDTO<CargaResultadosLoteResponseDTO>();
            var resultados = new CargaResultadosLoteResponseDTO
            {
                FechaProcesamiento = DateTime.Now,
                ResultadosExitosos = new List<CargaResultadoSubastaResponseDTO>(),
                Errores = new List<ErrorCargaResultadoDTO>()
            };

            try
            {
                // Procesamiento SECUENCIAL - uno por uno
                for (int i = 0; i < request.Resultados.Count; i++)
                {
                    var resultado = request.Resultados[i];
                    resultado.EmpId = request.EmpId;
                    resultado.PC = request.PC;

                    var numeroFila = i + 1;

                    _logger.Information("Procesando fila {NumeroFila}/{Total}: {CLPrendaCod}",
                        numeroFila, request.Resultados.Count, resultado.CLPrendaCod);

                    try
                    {
                        var responseItem = await CargarResultadoFilaAsync(resultado);
                        resultados.TotalProcesados++;

                        if (responseItem.Success)
                        {
                            resultados.TotalExitosos++;
                            resultados.ResultadosExitosos.Add(responseItem.Data);
                            _logger.Information("✓ Fila {NumeroFila} procesada exitosamente: {CLPrendaCod}",
                                numeroFila, resultado.CLPrendaCod);
                        }
                        else
                        {
                            resultados.TotalErrores++;
                            resultados.Errores.Add(new ErrorCargaResultadoDTO
                            {
                                CLPrendaCod = resultado.CLPrendaCod,
                                MensajeError = responseItem.Error?.Message ?? "Error desconocido",
                                NumeroFila = numeroFila
                            });
                            _logger.Warning("✗ Error en fila {NumeroFila}: {Error}",
                                numeroFila, responseItem.Error?.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "✗ Excepción en fila {NumeroFila} para prenda {CLPrendaCod}",
                            numeroFila, resultado.CLPrendaCod);

                        resultados.TotalProcesados++;
                        resultados.TotalErrores++;
                        resultados.Errores.Add(new ErrorCargaResultadoDTO
                        {
                            CLPrendaCod = resultado.CLPrendaCod,
                            MensajeError = $"Excepción: {ex.Message}",
                            NumeroFila = numeroFila
                        });
                    }
                }

                resultados.Mensaje = $"Procesamiento completado. {resultados.TotalExitosos} exitosos, {resultados.TotalErrores} errores de {resultados.TotalProcesados} total";

                response.Success = true;
                response.Data = resultados;
                response.Message = "Procesamiento de lote completado";

                _logger.Information("Lote procesado secuencialmente: {Total} total, {Exitosos} exitosos, {Errores} errores",
                    resultados.TotalProcesados, resultados.TotalExitosos, resultados.TotalErrores);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inesperado al procesar lote de resultados");

                response.Success = false;
                response.Error = new ErrorResponseDto
                {
                    ErrorCode = 500,
                    Message = "Error interno del servidor al procesar lote",
                    HttpStatusCode = 500
                };
                response.Message = "Error interno del servidor";

                return response;
            }
        }

        // ELIMINAR COMPLETAMENTE estos métodos si los tienes:
        // - ProcessSingleResultAsync
        // - ProcessSingleResultWithOwnContextAsync
        // - CargarResultadosLoteParaleloAsync


        private static (string Message, int? HttpStatusCode, int? BusinessCode) MapSqlErrorCargaResultado(int errorCode, string originalMessage)
        {
            return errorCode switch
            {
                50010 => ("La prenda no existe", 404, 50010),
                50011 => ("La prenda no está en estado 'En subasta'", 400, 50011),
                50012 => ("Estado inválido. Use: Rematado | Sin postor | No pagado", 400, 50012),
                50014 => ("La comuna indicada no existe en el sistema", 400, 50014),
                50015 => ("Para estado 'Rematado' debe proveer RUT del adjudicatario", 400, 50015),
                _ => (originalMessage, 400, errorCode)
            };
        }


    }
}
