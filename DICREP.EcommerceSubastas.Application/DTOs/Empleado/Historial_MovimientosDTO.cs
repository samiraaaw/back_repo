using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.DTOs.Empleado
{
    public class Historial_MovimientosDTO
    {
        public DateTime? MovFecha { get; set; }
        public string? MovTipoCambio { get; set; }
        public string? MovValorAnterior { get; set; }
        public string? MovValorNuevo { get; set; }
        public int? EmpId { get; set; }
    }
}
