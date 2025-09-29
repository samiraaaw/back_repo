using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICREP.EcommerceSubastas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Application.DTOs.Empleado
{
    public class EmpleadoDTO
    {
        public int EmpId { get; set; }
        public string EmpUsuario { get; set; } = null!;
        public int EmpRut { get; set; }
        public string EmpRutDig { get; set; } = null!;
        public string EmpNombre { get; set; } = null!;
        public string? EmpSegundoNombre { get; set; }
        public string EmpApellido { get; set; } = null!;
        public string? EmpSegundoApellido { get; set; }
        public string? EmpAnexo { get; set; }
        public string EmpCorreo { get; set; } = null!;
        public bool EmpActivo { get; set; }
        public DateTime? EmpFechaLog { get; set; }
        public DateTime? EmpFechaExp { get; set; }
        public int PerfilId { get; set; }
        public int SucursalId { get; set; }
        public int AuthMethod { get; set; }
        public string? PasswordHash { get; set; }
        public string? ClaveUnicaSub { get; set; }

    }
}

