using DICREP.EcommerceSubastas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.DTOs.Auth
{
    public class FuncionalidadDTO
    {
        public int FuncionalidadId { get; set; }
        public string Nombre { get; set; } = null!;
        public string EndpointApi { get; set; } = null!;
        public string MetodoHttp { get; set; } = null!;
        public string? Grupo { get; set; }
        public bool EsMenu { get; set; } = false;
        public bool Activo { get; set; } = true;
        public string Codigo { get; set; } = string.Empty!;
    }
}
