// DICREP.EcommerceSubastas.API/Controllers/CargaResultadoSubastaController.cs
using DICREP.EcommerceSubastas.API.Filters;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.DTOs.Subasta;
using DICREP.EcommerceSubastas.Application.UseCases.Subasta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Serilog;
using System.Linq;

namespace DICREP.EcommerceSubastas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CargaResultadoSubastaController : ControllerBase
    {
        private readonly CargaResultadoSubastaUseCase _cargaResultadoUseCase;
        private readonly Serilog.ILogger _logger;

        public CargaResultadoSubastaController(CargaResultadoSubastaUseCase cargaResultadoUseCase)
        {
            _cargaResultadoUseCase = cargaResultadoUseCase;
            _logger = Log.ForContext<CargaResultadoSubastaController>();

            // Configurar licencia de EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Carga el resultado de una subasta para una prenda específica
        /// </summary>
        [HttpPost("CargarResultado")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<ActionResult<ResponseDTO<CargaResultadoSubastaResponseDTO>>> CargarResultado(
            [FromBody] CargaResultadoSubastaRequestDTO request)
        {
            _logger.Information("Recibiendo carga de resultado para prenda {CLPrendaCod} con estado {Estado}",
                request?.CLPrendaCod, request?.EstadoTexto);

            var result = await _cargaResultadoUseCase.ExecuteAsync(request);

            if (!result.Success)
            {
                _logger.Warning("Error al cargar resultado para prenda {CLPrendaCod}: {ErrorMessage}",
                    request?.CLPrendaCod, result.Error?.Message);

                var statusCode = result.Error?.HttpStatusCode ?? StatusCodes.Status400BadRequest;
                return StatusCode(statusCode, result);
            }

            _logger.Information("Resultado cargado exitosamente para prenda {CLPrendaCod}",
                request?.CLPrendaCod);
            return Ok(result);
        }

        /// <summary>
        /// Carga múltiples resultados de subasta en lote
        /// </summary>
        [HttpPost("CargarResultadosLote")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<ActionResult<ResponseDTO<CargaResultadosLoteResponseDTO>>> CargarResultadosLote(
            [FromBody] CargaResultadosLoteRequestDTO request)
        {
            _logger.Information("Recibiendo carga de lote con {Count} resultados para empleado {EmpId}",
                request?.Resultados?.Count ?? 0, request?.EmpId);

            // LOG DETALLADO PARA DEBUG
            _logger.Debug("Request completo recibido: {@Request}", request);

            if (request?.EmpId == null || request.EmpId <= 0)
            {
                _logger.Warning("EmpId inválido o faltante: {EmpId}", request?.EmpId);
                return BadRequest(new ResponseDTO<CargaResultadosLoteResponseDTO>
                {
                    Success = false,
                    Message = "ID de empleado es requerido y debe ser mayor a 0",
                    Error = new ErrorResponseDto
                    {
                        ErrorCode = 400,
                        Message = "EmpId requerido",
                        HttpStatusCode = 400
                    }
                });
            }

            var result = await _cargaResultadoUseCase.ExecuteLoteAsync(request);

            if (!result.Success)
            {
                _logger.Warning("Error al procesar lote de resultados: {ErrorMessage}",
                    result.Error?.Message);

                var statusCode = result.Error?.HttpStatusCode ?? StatusCodes.Status400BadRequest;
                return StatusCode(statusCode, result);
            }

            _logger.Information("Lote procesado: {Total} total, {Exitosos} exitosos, {Errores} errores",
                result.Data?.TotalProcesados, result.Data?.TotalExitosos, result.Data?.TotalErrores);

            return Ok(result);
        }

        /// <summary>
        /// Carga resultados desde archivo Excel
        /// </summary>
        [HttpPost("CargarDesdeExcel")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseDTO<CargaResultadosLoteResponseDTO>>> CargarDesdeExcel(
            IFormFile file,
            [FromForm] int empId,
            [FromForm] string pc = null)
        {
            _logger.Information("Recibiendo archivo Excel. Archivo: {FileName}, EmpId: {EmpId}",
                file?.FileName, empId);

            if (file == null || file.Length == 0)
            {
                return BadRequest(new ResponseDTO<CargaResultadosLoteResponseDTO>
                {
                    Success = false,
                    Message = "Debe proporcionar un archivo Excel",
                    Error = new ErrorResponseDto
                    {
                        ErrorCode = 400,
                        Message = "Archivo requerido",
                        HttpStatusCode = 400
                    }
                });
            }

            if (empId <= 0)
            {
                return BadRequest(new ResponseDTO<CargaResultadosLoteResponseDTO>
                {
                    Success = false,
                    Message = "ID de empleado es requerido y debe ser mayor a 0",
                    Error = new ErrorResponseDto
                    {
                        ErrorCode = 400,
                        Message = "EmpId requerido",
                        HttpStatusCode = 400
                    }
                });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
            {
                return BadRequest(new ResponseDTO<CargaResultadosLoteResponseDTO>
                {
                    Success = false,
                    Message = "Solo se permiten archivos Excel (.xlsx o .xls)",
                    Error = new ErrorResponseDto
                    {
                        ErrorCode = 400,
                        Message = "Formato de archivo inválido",
                        HttpStatusCode = 400
                    }
                });
            }

            try
            {
                var excelData = await ProcessExcelFile(file);

                if (!excelData.Any())
                {
                    return BadRequest(new ResponseDTO<CargaResultadosLoteResponseDTO>
                    {
                        Success = false,
                        Message = "El archivo Excel no contiene datos válidos",
                        Error = new ErrorResponseDto
                        {
                            ErrorCode = 400,
                            Message = "No hay datos para procesar",
                            HttpStatusCode = 400
                        }
                    });
                }

                var loteRequest = new CargaResultadosLoteRequestDTO
                {
                    Resultados = excelData,
                    EmpId = empId,
                    PC = pc ?? "EXCEL_UPLOAD"
                };

                _logger.Information("Procesando {Count} registros desde Excel con EmpId {EmpId}",
                    excelData.Count, empId);

                var result = await _cargaResultadoUseCase.ExecuteLoteAsync(loteRequest);

                _logger.Information("Excel procesado. Total: {Total}, Exitosos: {Exitosos}, Errores: {Errores}",
                    result.Data?.TotalProcesados, result.Data?.TotalExitosos, result.Data?.TotalErrores);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error al procesar archivo Excel");
                return StatusCode(500, new ResponseDTO<CargaResultadosLoteResponseDTO>
                {
                    Success = false,
                    Message = "Error al procesar el archivo Excel",
                    Error = new ErrorResponseDto
                    {
                        ErrorCode = 500,
                        Message = ex.Message,
                        HttpStatusCode = 500
                    }
                });
            }
        }

        /// <summary>
        /// Analiza un archivo Excel sin procesarlo (solo para debug)
        /// </summary>
        [HttpPost("AnalyzarExcel")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AnalyzarExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se proporcionó archivo");

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);

                var worksheetAnalysis = new List<object>();

                foreach (var ws in package.Workbook.Worksheets)
                {
                    var headers = new List<object>();
                    var sampleData = new List<object>();

                    if (ws.Dimension != null && ws.Dimension.Rows > 0)
                    {
                        // Obtener headers (fila 1)
                        var maxCols = Math.Min(ws.Dimension.Columns, 20);
                        for (int col = 1; col <= maxCols; col++)
                        {
                            headers.Add(new
                            {
                                Column = col,
                                Value = ws.Cells[1, col]?.Text?.Trim() ?? ""
                            });
                        }

                        // Obtener datos de ejemplo (filas 2-4)
                        if (ws.Dimension.Rows > 1)
                        {
                            var maxRows = Math.Min(ws.Dimension.Rows, 4);
                            var maxSampleCols = Math.Min(ws.Dimension.Columns, 10);

                            for (int row = 2; row <= maxRows; row++)
                            {
                                var rowData = new List<string>();
                                for (int col = 1; col <= maxSampleCols; col++)
                                {
                                    rowData.Add(ws.Cells[row, col]?.Text?.Trim() ?? "");
                                }
                                sampleData.Add(new { Row = row, Data = rowData });
                            }
                        }
                    }

                    worksheetAnalysis.Add(new
                    {
                        Name = ws.Name,
                        HasDimension = ws.Dimension != null,
                        Dimension = ws.Dimension?.Address ?? "NULL",
                        Rows = ws.Dimension?.Rows ?? 0,
                        Columns = ws.Dimension?.Columns ?? 0,
                        Headers = headers,
                        SampleData = sampleData
                    });
                }

                var analysis = new
                {
                    FileName = file.FileName,
                    FileSize = file.Length,
                    WorksheetCount = package.Workbook.Worksheets.Count,
                    Worksheets = worksheetAnalysis
                };

                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error al analizar Excel");
                return BadRequest(new
                {
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// Obtiene el formato esperado para Excel
        /// </summary>
        [HttpGet("FormatoExcel")]
        public ActionResult<object> GetFormatoExcel()
        {
            var formato = new
            {
                NombreArchivo = "ResultadosSubasta.xlsx",
                Descripcion = "Formato de archivo Excel para cargar resultados de subastas",
                Columnas = new[]
                {
                    new { Nombre = "CLPrenda_Cod", Tipo = "Texto", Requerido = true, Descripcion = "Código de la prenda" },
                    new { Nombre = "CLPrenda_Subasta", Tipo = "Número", Requerido = true, Descripcion = "Número de la subasta" },
                    new { Nombre = "EstadoTexto", Tipo = "Texto", Requerido = true, Descripcion = "Estado: 'Rematado', 'Sin postor' o 'No pagado'" },
                    new { Nombre = "MontoAdjudicacion", Tipo = "Decimal", Requerido = false, Descripcion = "Monto de adjudicación" },
                    new { Nombre = "Adj_Rut", Tipo = "Texto", Requerido = false, Descripcion = "RUT del adjudicatario (requerido si estado es 'Rematado')" },
                    new { Nombre = "Adj_Nombre", Tipo = "Texto", Requerido = false, Descripcion = "Nombre del adjudicatario" },
                    new { Nombre = "Adj_ApellidoP", Tipo = "Texto", Requerido = false, Descripcion = "Apellido paterno del adjudicatario" }
                },
                Ejemplo = new
                {
                    Fila1 = "7|33|Rematado|100|12345678-9|Juan|Pérez",
                    Fila2 = "8|33|Sin postor||||",
                    Fila3 = "9|33|No pagado|300|98765432-1|María|González"
                }
            };

            return Ok(formato);
        }

        /// <summary>
        /// Procesa el archivo Excel y extrae los datos de resultados de subasta
        /// </summary>
        private async Task<List<CargaResultadoSubastaRequestDTO>> ProcessExcelFile(IFormFile file)
        {
            var resultados = new List<CargaResultadoSubastaRequestDTO>();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    throw new InvalidOperationException("El archivo Excel no contiene hojas de trabajo válidas");
                }

                // Verificar que hay datos
                if (worksheet.Dimension == null || worksheet.Dimension.End.Row < 2)
                {
                    throw new InvalidOperationException("El archivo Excel no contiene datos válidos");
                }

                // Mapear las columnas (asumiendo que la primera fila contiene los headers)
                var headers = new Dictionary<string, int>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var headerValue = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        headers[headerValue] = col;
                    }
                }

                // Verificar columnas requeridas
                var columnasRequeridas = new[] { "CLPrenda_Cod", "CLPrenda_Subasta" };
                var columnasFaltantes = columnasRequeridas.Where(col => !headers.ContainsKey(col)).ToList();

                if (columnasFaltantes.Any())
                {
                    throw new InvalidOperationException($"Faltan las siguientes columnas requeridas: {string.Join(", ", columnasFaltantes)}");
                }

                _logger.Information("Columnas encontradas en Excel: {Headers}", string.Join(", ", headers.Keys));

                // Procesar cada fila de datos (empezando desde la fila 2)
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    try
                    {
                        // Leer valores de las celdas
                        var clPrendaCod = worksheet.Cells[row, headers["CLPrenda_Cod"]].Value?.ToString()?.Trim();
                        var clPrendaSubasta = worksheet.Cells[row, headers["CLPrenda_Subasta"]].Value?.ToString()?.Trim();

                        // Validar datos obligatorios
                        if (string.IsNullOrEmpty(clPrendaCod))
                        {
                            _logger.Warning("Fila {Row}: CLPrenda_Cod está vacío, saltando", row);
                            continue;
                        }

                        if (string.IsNullOrEmpty(clPrendaSubasta) || !int.TryParse(clPrendaSubasta, out int numeroSubasta))
                        {
                            _logger.Warning("Fila {Row}: CLPrenda_Subasta inválido o vacío: {Value}", row, clPrendaSubasta);
                            continue;
                        }

                        var resultado = new CargaResultadoSubastaRequestDTO
                        {
                            CLPrendaCod = clPrendaCod,
                            CLPrendaSubasta = numeroSubasta,
                            // Otros campos opcionales
                            FechaSub = TryGetDateTimeValue(worksheet, row, headers, "FechaSub"),
                            EstadoTexto = TryGetStringValue(worksheet, row, headers, "EstadoTexto"),
                            MontoMinimo = TryGetDecimalValue(worksheet, row, headers, "MontoMinino"), // Nota: el Excel tiene "MontoMinino" 
                            MontoAdjudicacion = TryGetDecimalValue(worksheet, row, headers, "MontoAdjudicacion"),
                            TotalAdjudicacion = TryGetDecimalValue(worksheet, row, headers, "TotalAdjudicacion"),
                            FechaAdjudicacion = TryGetDateTimeValue(worksheet, row, headers, "FechaAdjudicacion")
                        };

                        resultados.Add(resultado);
                        _logger.Information("Fila {Row} procesada: CLPrendaCod={CLPrendaCod}, CLPrendaSubasta={CLPrendaSubasta}",
                            row, resultado.CLPrendaCod, resultado.CLPrendaSubasta);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "Error procesando fila {Row}, continuando con la siguiente", row);
                        continue;
                    }
                }

                _logger.Information("ProcessExcelFile completado: {Count} registros extraídos", resultados.Count);
                return resultados;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error al procesar archivo Excel: {FileName}", file.FileName);
                throw;
            }
        }

        /// <summary>
        /// Métodos auxiliares para extraer valores de forma segura
        /// </summary>
        private string TryGetStringValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> headers, string columnName)
        {
            if (!headers.ContainsKey(columnName)) return null;
            return worksheet.Cells[row, headers[columnName]].Value?.ToString()?.Trim();
        }

        private decimal? TryGetDecimalValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> headers, string columnName)
        {
            if (!headers.ContainsKey(columnName)) return null;
            var value = worksheet.Cells[row, headers[columnName]].Value;
            if (value != null && decimal.TryParse(value.ToString(), out decimal result))
                return result;
            return null;
        }

        private DateTime? TryGetDateTimeValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> headers, string columnName)
        {
            if (!headers.ContainsKey(columnName)) return null;
            var value = worksheet.Cells[row, headers[columnName]].Value;

            if (value is DateTime dateTime)
                return dateTime;

            if (value != null && DateTime.TryParse(value.ToString(), out DateTime result))
                return result;

            return null;
        }
    }
}