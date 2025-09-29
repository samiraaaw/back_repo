using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.DTOs.Empleado
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public string Usuario { get; set; }
        public string NombreCompleto { get; set; }
        public int PerfilId { get; set; }
        public int SucursalId { get; set; }
        public DateTime Expiration { get; set; }
    }
}
