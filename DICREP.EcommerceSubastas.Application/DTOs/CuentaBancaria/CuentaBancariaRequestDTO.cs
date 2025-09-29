using System.ComponentModel.DataAnnotations;

namespace DICREP.EcommerceSubastas.Application.DTOs.CuentaBancaria
{
    public class CuentaBancariaRequestDTO
    {
        [Required(ErrorMessage = "El ID del organismo es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del organismo debe ser mayor a 0")]
        public int OrganismoId { get; set; }

        [Required(ErrorMessage = "El ID del banco es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del banco debe ser mayor a 0")]
        public int BancoId { get; set; }

        [Required(ErrorMessage = "El ID del tipo de cuenta es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del tipo de cuenta debe ser mayor a 0")]
        public int TipoCuentaId { get; set; }

        [Required(ErrorMessage = "El número de cuenta es requerido")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El número de cuenta debe tener entre 1 y 50 caracteres")]
        public string NumeroCuenta { get; set; }

        [Required(ErrorMessage = "El nombre de la cuenta es requerido")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "El nombre de la cuenta debe tener entre 1 y 150 caracteres")]
        public string NombreCuenta { get; set; }

        [StringLength(20, ErrorMessage = "El RUT no puede exceder 20 caracteres")]
        public string? Rut { get; set; }

        [StringLength(255, ErrorMessage = "El correo no puede exceder 255 caracteres")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string? Correo { get; set; }

        [StringLength(100, ErrorMessage = "El usuario no puede exceder 100 caracteres")]
        public string? Usuario { get; set; }

        [StringLength(50, ErrorMessage = "El origen no puede exceder 50 caracteres")]
        public string? Origen { get; set; }
    }

    public class CuentaBancariaResponseDTO
    {
        public int CuentaId { get; set; }
        public string Mensaje { get; set; }
        public bool EsNueva { get; set; }
    }
}