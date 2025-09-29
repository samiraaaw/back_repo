using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// DTOs/Subasta/PrendaValidacionErrorDTO.cs
namespace DICREP.EcommerceSubastas.Application.DTOs.Subasta
{
    public class PrendaValidacionErrorDTO
    {
        public long CLPrendaId { get; set; }
        public string CLPrendaCod { get; set; }
        public string Motivo { get; set; }
    }
}