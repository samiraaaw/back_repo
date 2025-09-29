using System.ComponentModel.DataAnnotations;

namespace DICREP.EcommerceSubastas.Application.DTOs.CLPrenda
{
    public class CLPrendaUpdateRequestDTO
    {
        [Required(ErrorMessage = "El código de la prenda es requerido")]
        [StringLength(64, MinimumLength = 1, ErrorMessage = "El código debe tener entre 1 y 64 caracteres")]
        public string CLPrendaCod { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El incremento debe ser mayor o igual a 0")]
        public decimal? ValorIncremento { get; set; }

        [Range(0, 100, ErrorMessage = "La comisión debe estar entre 0 y 100")]
        public decimal? ValorComision { get; set; }

        [Required(ErrorMessage = "El ID del empleado es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del empleado debe ser mayor a 0")]
        public int EmpId { get; set; }

        [StringLength(50, ErrorMessage = "El PC no puede exceder 50 caracteres")]
        public string? PC { get; set; }
    }

    public class CLPrendaUpdateResponseDTO
    {
        public long CLPrendaId { get; set; }
        public string CLPrendaCod { get; set; }
        public string CLPrendaNombre { get; set; }
        public decimal? CLPrendaIncremento { get; set; }
        public decimal? CLPrendaComision { get; set; }
        public string Mensaje { get; set; }
    }
}