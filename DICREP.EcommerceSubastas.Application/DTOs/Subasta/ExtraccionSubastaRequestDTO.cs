using System.ComponentModel.DataAnnotations;

namespace DICREP.EcommerceSubastas.Application.DTOs.Subasta
{
    public class ExtraccionSubastaRequestDTO
    {
        [DataType(DataType.Date)]
        public DateTime? FechaDesde { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaHasta { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El ID del organismo debe ser mayor a 0")]
        public int? OrganismoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El ID del estado del bien debe ser mayor a 0")]
        public int? EstBienId { get; set; }

        [Required(ErrorMessage = "El ID del empleado es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del empleado debe ser mayor a 0")]
        public int EmpId { get; set; }

        [StringLength(50, ErrorMessage = "El PC no puede exceder 50 caracteres")]
        public string? PC { get; set; }

        [Range(0, 100, ErrorMessage = "La comisión debe estar entre 0 y 100")]
        public decimal? Comision { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El incremento debe ser mayor o igual a 0")]
        public decimal? Incremento { get; set; }

        // Validación personalizada para rango de fechas
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaDesde.HasValue && FechaHasta.HasValue && FechaDesde > FechaHasta)
            {
                yield return new ValidationResult(
                    "La fecha desde no puede ser mayor que la fecha hasta",
                    new[] { nameof(FechaDesde), nameof(FechaHasta) }
                );
            }
        }
    }
}