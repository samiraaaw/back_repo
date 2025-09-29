// DICREP.EcommerceSubastas.Application/Services/ExcelProcessorService.cs
using DICREP.EcommerceSubastas.Application.DTOs.Subasta;
using System.Globalization;
using Serilog;

namespace DICREP.EcommerceSubastas.Application.Services
{
    public class ExcelProcessorService
    {
        private readonly ILogger _logger;

        public ExcelProcessorService()
        {
            _logger = Log.ForContext<ExcelProcessorService>();
        }

        public class ExcelProcessResult
        {
            public bool Success { get; set; }
            public List<CargaResultadoSubastaRequestDTO> Resultados { get; set; } = new();
            public List<string> Errores { get; set; } = new();
            public int TotalFilas { get; set; }
        }

        /// <summary>
        /// Procesa los datos de un Excel convertidos a lista de diccionarios
        /// </summary>
        /// <param name="excelData">Datos del Excel por fila</param>
        /// <param name="empId">ID del empleado que realiza la carga</param>
        /// <param name="pc">PC desde donde se realiza la carga</param>
        /// <returns>Resultado del procesamiento</returns>
        public ExcelProcessResult ProcessExcelData(List<Dictionary<string, object>> excelData, int empId, string pc = null)
        {
            var result = new ExcelProcessResult();
            var errores = new List<string>();

            _logger.Information("Iniciando procesamiento de Excel con {Count} filas", excelData.Count);

            for (int i = 0; i < excelData.Count; i++)
            {
                var fila = i + 2; // Empezar desde fila 2 (asumiendo que fila 1 son headers)
                var row = excelData[i];

                try
                {
                    var dto = MapRowToDTO(row, empId, pc);
                    if (dto != null)
                    {
                        result.Resultados.Add(dto);
                    }
                    else
                    {
                        errores.Add($"Fila {fila}: No se pudo procesar la fila");
                    }
                }
                catch (Exception ex)
                {
                    errores.Add($"Fila {fila}: {ex.Message}");
                    _logger.Warning(ex, "Error al procesar fila {Fila}", fila);
                }
            }

            result.TotalFilas = excelData.Count;
            result.Errores = errores;
            result.Success = errores.Count == 0;

            _logger.Information("Procesamiento completado. {Exitosos}/{Total} filas procesadas correctamente",
                result.Resultados.Count, result.TotalFilas);

            return result;
        }

        private CargaResultadoSubastaRequestDTO MapRowToDTO(Dictionary<string, object> row, int empId, string pc)
        {
            var dto = new CargaResultadoSubastaRequestDTO
            {
                EmpId = empId,
                PC = pc
            };

            // Mapear campos requeridos
            dto.CLPrendaCod = GetStringValue(row, "CLPrenda_Cod")?.Trim();
            if (string.IsNullOrEmpty(dto.CLPrendaCod))
            {
                throw new ArgumentException("Código de prenda es requerido");
            }

            dto.EstadoTexto = GetStringValue(row, "EstadoTexto")?.Trim();
            if (string.IsNullOrEmpty(dto.EstadoTexto))
            {
                throw new ArgumentException("Estado es requerido");
            }

            // Validar estado
            var estadosValidos = new[] { "Rematado", "Sin postor", "No pagado" };
            if (!estadosValidos.Contains(dto.EstadoTexto, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Estado inválido. Use: {string.Join(", ", estadosValidos)}");
            }

            // Mapear campos opcionales
            dto.CLPrendaSubasta = GetIntValue(row, "CLPrenda_Subasta");
            dto.FechaSub = GetDateTimeValue(row, "FechaSub");
            dto.MontoMinimo = GetDecimalValue(row, "MontoMinimo");
            dto.MontoAdjudicacion = GetDecimalValue(row, "MontoAdjudicacion");
            dto.TotalAdjudicacion = GetDecimalValue(row, "TotalAdjudicacion");
            dto.FechaAdjudicacion = GetDateTimeValue(row, "FechaAdjudicacion");
            dto.Moneda = GetStringValue(row, "Moneda")?.Trim();
            dto.ComisionPorc = GetDecimalValue(row, "ComisionPorc");
            dto.TotalComision = GetDecimalValue(row, "TotalComision");
            dto.IvaComision = GetDecimalValue(row, "IvaComision");
            dto.TotalRecaudar = GetDecimalValue(row, "TotalRecaudar");

            // Datos del adjudicatario
            dto.AdjRut = GetStringValue(row, "Adj_Rut")?.Trim();
            dto.AdjNombre = GetStringValue(row, "Adj_Nombre")?.Trim();
            dto.AdjNombreS = GetStringValue(row, "Adj_NombreS")?.Trim();
            dto.AdjApellidoP = GetStringValue(row, "Adj_ApellidoP")?.Trim();
            dto.AdjApellidoM = GetStringValue(row, "Adj_ApellidoM")?.Trim();
            dto.AdjCorreo = GetStringValue(row, "Adj_Correo")?.Trim();
            dto.AdjComunaNombre = GetStringValue(row, "Adj_ComunaNombre")?.Trim();

            // Validación específica para estado "Rematado"
            if (string.Equals(dto.EstadoTexto, "Rematado", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(dto.AdjRut))
                {
                    throw new ArgumentException("Para estado 'Rematado' es obligatorio proporcionar el RUT del adjudicatario");
                }
            }

            return dto;
        }

        private string GetStringValue(Dictionary<string, object> row, string columnName)
        {
            if (row.TryGetValue(columnName, out var value) && value != null)
            {
                return value.ToString();
            }
            return null;
        }

        private int? GetIntValue(Dictionary<string, object> row, string columnName)
        {
            if (row.TryGetValue(columnName, out var value) && value != null)
            {
                if (int.TryParse(value.ToString(), out int result))
                {
                    return result;
                }
            }
            return null;
        }

        private decimal? GetDecimalValue(Dictionary<string, object> row, string columnName)
        {
            if (row.TryGetValue(columnName, out var value) && value != null)
            {
                if (decimal.TryParse(value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
                {
                    return result;
                }
            }
            return null;
        }

        private DateTime? GetDateTimeValue(Dictionary<string, object> row, string columnName)
        {
            if (row.TryGetValue(columnName, out var value) && value != null)
            {
                var stringValue = value.ToString();

                // Intentar varios formatos de fecha
                var formats = new[]
                {
                    "dd/MM/yyyy",
                    "dd/MM/yyyy HH:mm:ss",
                    "yyyy-MM-dd",
                    "yyyy-MM-dd HH:mm:ss",
                    "MM/dd/yyyy",
                    "MM/dd/yyyy HH:mm:ss"
                };

                foreach (var format in formats)
                {
                    if (DateTime.TryParseExact(stringValue, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    {
                        return result;
                    }
                }

                // Intentar parsing genérico
                if (DateTime.TryParse(stringValue, out DateTime genericResult))
                {
                    return genericResult;
                }
            }
            return null;
        }
    }
}