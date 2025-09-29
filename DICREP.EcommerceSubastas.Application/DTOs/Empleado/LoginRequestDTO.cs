using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.DTOs.Empleado
{
    public class LoginRequestDTO
    {
        public string Usuario { get; set; }
        public string Password { get; set; }
        public int AuthMethod { get; set; }
    }
}
