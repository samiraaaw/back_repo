using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// DICREP.EcommerceSubastas.Application/DTOs/Subasta/CargaResultadoSubastaDTO.cs
using System.ComponentModel.DataAnnotations;

namespace DICREP.EcommerceSubastas.Application.DTOs.Subasta
{
    public class CargaResultadoSubastaRequestDTO
    {
        [Required(ErrorMessage = "El código de la prenda es requerido")]
        [StringLength(64, ErrorMessage = "El código de la prenda no puede exceder 64 caracteres")]
        public string CLPrendaCod { get; set; }

        [Required(ErrorMessage = "El número de subasta es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El número de subasta debe ser mayor a 0")]
        public int? CLPrendaSubasta { get; set; } // PROPIEDAD CORREGIDA


        [DataType(DataType.DateTime)]
        public DateTime? FechaSub { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [RegularExpression(@"^(Rematado|Sin postor|No pagado)$",
            ErrorMessage = "Estado inválido. Use: Rematado, Sin postor o No pagado")]
        public string EstadoTexto { get; set; }

        // Valores económicos
        [Range(0, double.MaxValue, ErrorMessage = "El monto mínimo debe ser mayor o igual a 0")]
        public decimal? MontoMinimo { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El monto de adjudicación debe ser mayor o igual a 0")]
        public decimal? MontoAdjudicacion { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El total de adjudicación debe ser mayor o igual a 0")]
        public decimal? TotalAdjudicacion { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? FechaAdjudicacion { get; set; }

        [StringLength(20, ErrorMessage = "La moneda no puede exceder 20 caracteres")]
        public string? Moneda { get; set; }

        [Range(0, 100, ErrorMessage = "La comisión debe estar entre 0 y 100")]
        public decimal? ComisionPorc { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El total de comisión debe ser mayor o igual a 0")]
        public decimal? TotalComision { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El IVA de comisión debe ser mayor o igual a 0")]
        public decimal? IvaComision { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El total a recaudar debe ser mayor o igual a 0")]
        public decimal? TotalRecaudar { get; set; }

        // Adjudicatario (solo requerido si estado es "Rematado")
        [StringLength(20, ErrorMessage = "El RUT no puede exceder 20 caracteres")]
        public string? AdjRut { get; set; }

        [StringLength(20, ErrorMessage = "El nombre no puede exceder 20 caracteres")]
        public string? AdjNombre { get; set; }

        [StringLength(20, ErrorMessage = "El segundo nombre no puede exceder 20 caracteres")]
        public string? AdjNombreS { get; set; }

        [StringLength(20, ErrorMessage = "El apellido paterno no puede exceder 20 caracteres")]
        public string? AdjApellidoP { get; set; }

        [StringLength(20, ErrorMessage = "El apellido materno no puede exceder 20 caracteres")]
        public string? AdjApellidoM { get; set; }

        [StringLength(255, ErrorMessage = "El correo no puede exceder 255 caracteres")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string? AdjCorreo { get; set; }

        [StringLength(150, ErrorMessage = "El nombre de la comuna no puede exceder 150 caracteres")]
        public string? AdjComunaNombre { get; set; }

        // Auditoría
        [Required(ErrorMessage = "El ID del empleado es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del empleado debe ser mayor a 0")]
        public int EmpId { get; set; }

        [StringLength(50, ErrorMessage = "El PC no puede exceder 50 caracteres")]
        public string? PC { get; set; }
    }

    public class CargaResultadoSubastaResponseDTO
    {
        public string CLPrendaCod { get; set; }
        public string EstadoAnterior { get; set; }
        public string EstadoNuevo { get; set; }
        public bool TieneAdjudicatario { get; set; }
        public int? AdjudicatarioId { get; set; }
        public string Mensaje { get; set; }
        public DateTime FechaProcesamiento { get; set; }
    }

        public class CargaResultadosLoteRequestDTO
        {
            [Required(ErrorMessage = "La lista de resultados es requerida")]
            [MinLength(1, ErrorMessage = "Debe proporcionar al menos un resultado")]
            public List<CargaResultadoSubastaRequestDTO> Resultados { get; set; } = new();

            [Required(ErrorMessage = "El ID del empleado es requerido")]
            [Range(1, int.MaxValue, ErrorMessage = "El ID del empleado debe ser mayor a 0")]
            public int EmpId { get; set; }

            [StringLength(50, ErrorMessage = "El PC no puede exceder 50 caracteres")]
            public string? PC { get; set; }
        }

    public class CargaResultadosLoteResponseDTO
    {
        public int TotalProcesados { get; set; }
        public int TotalExitosos { get; set; }
        public int TotalErrores { get; set; }
        public List<CargaResultadoSubastaResponseDTO> ResultadosExitosos { get; set; } = new();
        public List<ErrorCargaResultadoDTO> Errores { get; set; } = new();
        public string Mensaje { get; set; }
        public DateTime FechaProcesamiento { get; set; }
    }

    public class ErrorCargaResultadoDTO
    {
        public string CLPrendaCod { get; set; }
        public string MensajeError { get; set; }
        public int NumeroFila { get; set; }
    }
}