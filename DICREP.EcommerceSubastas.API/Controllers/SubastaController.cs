using DICREP.EcommerceSubastas.API.Filters;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.DTOs.Subasta;
using DICREP.EcommerceSubastas.Application.UseCases.Subasta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Serilog;
using System.Drawing;


namespace DICREP.EcommerceSubastas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class SubastaController : ControllerBase
    {
        private readonly ExtraccionSubastaUseCase _extraccionSubastaUseCase;
        private readonly Serilog.ILogger _logger;
        private readonly CargaResultadoSubastaUseCase _cargaResultadoUseCase; // NUEVA LÍNEA



        public SubastaController(
        ExtraccionSubastaUseCase extraccionSubastaUseCase,
        CargaResultadoSubastaUseCase cargaResultadoUseCase) // NUEVO PARÁMETRO
        {
            _extraccionSubastaUseCase = extraccionSubastaUseCase;
            _cargaResultadoUseCase = cargaResultadoUseCase; // NUEVA ASIGNACIÓN
            _logger = Log.ForContext<SubastaController>();
        }

        /// <summary>
        /// Extrae prendas para subasta y cambia su estado a "En subasta"
        /// </summary>
        /// <param name="request">Filtros de extracción</param>
        /// <returns>Lista de prendas extraídas con toda su información</returns>
        [HttpPost("ExtraccionSubasta")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO<ExtraccionSubastaResponseDTO>>> ExtraccionSubasta(
            [FromBody] ExtraccionSubastaRequestDTO request)
        {
            _logger.Information("Recibiendo solicitud de extracción de subasta para empleado {EmpId}",
                request?.EmpId);

            var result = await _extraccionSubastaUseCase.ExecuteAsync(request);

            if (!result.Success)
            {
                _logger.Warning("Error en extracción de subasta: {ErrorMessage}",
                    result.Error?.Message);

                var statusCode = result.Error?.HttpStatusCode ?? StatusCodes.Status400BadRequest;
                return StatusCode(statusCode, result);
            }

            _logger.Information("Extracción de subasta completada. Total extraídas: {Total}",
                result.Data?.TotalExtraidas);

            return Ok(result);
        }

        /// <summary>
        /// Obtiene un resumen de prendas disponibles para extracción (sin cambiar estado)
        /// </summary>
        /// <param name="fechaDesde">Fecha desde (opcional)</param>
        /// <param name="fechaHasta">Fecha hasta (opcional)</param>
        /// <param name="organismoId">ID del organismo (opcional)</param>
        /// <param name="estBienId">ID del estado del bien (opcional)</param>
        /// <returns>Conteo de prendas que se extraerían</returns>
        [HttpGet("Preview")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> PreviewExtraccion(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int? organismoId = null,
            [FromQuery] int? estBienId = null)
        {
            // Este endpoint podría implementarse para mostrar un preview
            // sin ejecutar la extracción real
            _logger.Information("Preview de extracción solicitado");

            // Implementación simple - podrías crear otro SP o consulta
            return Ok(new
            {
                mensaje = "Funcionalidad de preview no implementada",
                sugerencia = "Use el endpoint de extracción con un empleado de prueba"
            });
        }



        // Y agregar este método al final de la clase:

        /// <summary>
        /// Carga el resultado final de una subasta
        /// </summary>
        /// <param name="request">Datos del resultado de la subasta</param>
        /// <returns>Información del resultado procesado</returns>
        [HttpPost("CargarResultado")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO<CargaResultadoSubastaResponseDTO>>> CargarResultado(
            [FromBody] CargaResultadoSubastaRequestDTO request)
        {
            _logger.Information("Cargando resultado de subasta para prenda {CLPrendaCod}",
                request?.CLPrendaCod);

            var result = await _cargaResultadoUseCase.ExecuteAsync(request);

            if (!result.Success)
            {
                _logger.Warning("Error al cargar resultado: {ErrorMessage}",
                    result.Error?.Message);

                var statusCode = result.Error?.HttpStatusCode ?? StatusCodes.Status400BadRequest;
                return StatusCode(statusCode, result);
            }

            _logger.Information("Resultado de subasta cargado exitosamente para prenda {CLPrendaCod}",
                request?.CLPrendaCod);

            return Ok(result);
        }






        // Y agregar estos métodos al SubastaController.cs

        /// <summary>
        /// Extrae prendas para subasta y las exporta directamente a Excel
        /// </summary>
        /// <param name="request">Filtros de extracción</param>
        /// <returns>Archivo Excel con las prendas extraídas</returns>
        [HttpPost("ExtraccionSubastaExcel")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        [AllowAnonymous]
        public async Task<IActionResult> ExtraccionSubastaExcel([FromBody] ExtraccionSubastaRequestDTO request)
        {
            _logger.Information("Iniciando extracción y exportación a Excel para empleado {EmpId}", request?.EmpId);

            try
            {
                // 1. Realizar la extracción normal
                var result = await _extraccionSubastaUseCase.ExecuteAsync(request);

                if (!result.Success)
                {
                    _logger.Warning("Error en la extracción para Excel: {ErrorMessage}", result.Error?.Message);
                    return BadRequest(new { message = "Error en la extracción", error = result.Error });
                }

                // 2. Generar Excel (incluso si no hay datos)
                var excelBytes = GenerateExcelFromExtraction(result.Data);

                // 3. Retornar archivo
                var fileName = $"ExtraccionSubasta_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                _logger.Information("Excel generado exitosamente. Tamaño: {Size} bytes", excelBytes.Length);

                return File(
                    excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error al generar Excel de extracción");
                return StatusCode(500, new { message = "Error interno al generar Excel", error = ex.Message });
            }
        }

        /// <summary>
        /// Genera el archivo Excel con los datos de extracción
        /// </summary>
        private byte[] GenerateExcelFromExtraction(ExtraccionSubastaResponseDTO data)
        {
            // Configurar EPPlus para uso no comercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Extracción Subasta");

            // HEADERS - Fila 1
            var headers = new[]
            {
        "CLPrenda_Codigo", "Fecha_Ingreso_Fase2", "Fecha_Termino_Fase2", "Categoria",
        "NombreProducto", "Descripcion", "Estado_Bien", "Cantidad",
        "Precio_Total", "Comision", "Incremento", "Nombre_Organizacion",
        "Rut_Organizacion", "Correo", "Telefono", "Comuna",
        "Direccion_Contacto", "Region", "Foto1", "Foto2", "Foto3",
        "Foto4", "Foto5", "Foto6", "Informe1", "Informe2", "Informe3",
        "Informe4", "Informe5", "Informe6"
    };

            // Crear headers
            for (int i = 0; i < headers.Length; i++)
            {
                var headerCell = worksheet.Cells[1, i + 1];
                headerCell.Value = headers[i];
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerCell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                headerCell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            }

            var row = 2;
            var totalRows = 1; // Al menos los headers

            // DATOS - Desde fila 2 (si hay datos)
            if (data?.PrendasExtraidas?.Any() == true)
            {
                foreach (var prenda in data.PrendasExtraidas)
                {
                    var col = 1;
                    worksheet.Cells[row, col++].Value = prenda.CLPrendaCodigo ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.FechaIngresoFase2?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.FechaTerminoFase2?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Categoria ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.NombreProducto ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Descripcion ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.EstadoBien ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Cantidad ?? 0;
                    worksheet.Cells[row, col++].Value = prenda.PrecioTotal ?? 0;
                    worksheet.Cells[row, col++].Value = prenda.Comision ?? 0;
                    worksheet.Cells[row, col++].Value = prenda.Incremento ?? 0;
                    worksheet.Cells[row, col++].Value = prenda.NombreOrganizacion ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.RutOrganizacion ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Correo ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Telefono ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Comuna ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.DireccionContacto ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Region ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Foto1 ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Foto2 ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Foto3 ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Foto4 ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Foto5 ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Foto6 ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Informe1 ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Informe2 ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Informe3 ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Informe4 ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Informe5 ?? string.Empty;
                    worksheet.Cells[row, col++].Value = prenda.Informe6 ?? string.Empty;

                    row++;
                }
                totalRows = row - 1;
            }
            else
            {
                // Si no hay datos, agregar una fila vacía para evitar errores
                worksheet.Cells[2, 1].Value = "No se encontraron prendas para extraer";
                totalRows = 2;
            }

            // FORMATEO SEGURO
            try
            {
                // Auto-ajustar columnas de manera segura
                if (totalRows > 0 && headers.Length > 0)
                {
                    var range = worksheet.Cells[1, 1, totalRows, headers.Length];
                    range.AutoFitColumns(5, 50); // Min: 5, Max: 50 caracteres de ancho
                }

                // Formatear fechas solo si hay datos
                if (totalRows > 1)
                {
                    var fechaColumns = new[] { 2, 3 }; // Columnas de fecha
                    foreach (var fechaCol in fechaColumns)
                    {
                        var fechaRange = worksheet.Cells[2, fechaCol, totalRows, fechaCol];
                        fechaRange.Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                    }

                    // Formatear números
                    var moneyColumns = new[] { 9, 10, 11 }; // Precio, Comisión, Incremento
                    foreach (var moneyCol in moneyColumns)
                    {
                        var moneyRange = worksheet.Cells[2, moneyCol, totalRows, moneyCol];
                        moneyRange.Style.Numberformat.Format = "#,##0";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error al formatear Excel, continuando sin formateo avanzado");
            }

            // HOJA DE RESUMEN
            try
            {
                var summarySheet = package.Workbook.Worksheets.Add("Resumen");

                summarySheet.Cells[1, 1].Value = "RESUMEN DE EXTRACCIÓN";
                summarySheet.Cells[1, 1].Style.Font.Bold = true;
                summarySheet.Cells[1, 1].Style.Font.Size = 16;

                summarySheet.Cells[3, 1].Value = "Total extraídas:";
                summarySheet.Cells[3, 2].Value = data?.TotalExtraidas ?? 0;
                summarySheet.Cells[3, 2].Style.Font.Bold = true;

                summarySheet.Cells[4, 1].Value = "Fecha de extracción:";
                summarySheet.Cells[4, 2].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                summarySheet.Cells[5, 1].Value = "Errores:";
                summarySheet.Cells[5, 2].Value = data?.ErroresValidacion?.Count ?? 0;
                if ((data?.ErroresValidacion?.Count ?? 0) > 0)
                {
                    summarySheet.Cells[5, 2].Style.Font.Color.SetColor(Color.Red);
                    summarySheet.Cells[5, 2].Style.Font.Bold = true;
                }

                if (data?.ErroresValidacion?.Any() == true)
                {
                    summarySheet.Cells[7, 1].Value = "ERRORES ENCONTRADOS:";
                    summarySheet.Cells[7, 1].Style.Font.Bold = true;
                    summarySheet.Cells[7, 1].Style.Font.Color.SetColor(Color.Red);

                    summarySheet.Cells[8, 1].Value = "Código";
                    summarySheet.Cells[8, 2].Value = "Motivo";
                    summarySheet.Cells[8, 1].Style.Font.Bold = true;
                    summarySheet.Cells[8, 2].Style.Font.Bold = true;

                    var errorRow = 9;
                    foreach (var error in data.ErroresValidacion)
                    {
                        summarySheet.Cells[errorRow, 1].Value = error.CLPrendaCod ?? string.Empty;
                        summarySheet.Cells[errorRow, 2].Value = error.Motivo ?? string.Empty;
                        errorRow++;
                    }
                }

                // Auto-ajustar resumen de manera segura
                summarySheet.Cells[1, 1, 20, 2].AutoFitColumns(10, 100);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error al crear hoja de resumen, continuando sin resumen");
            }

            return package.GetAsByteArray();
        }
    }
}
