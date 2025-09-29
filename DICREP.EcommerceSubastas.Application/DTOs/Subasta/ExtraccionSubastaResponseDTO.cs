using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// DTOs/Subasta/ExtraccionSubastaResponseDTO.cs
namespace DICREP.EcommerceSubastas.Application.DTOs.Subasta
{
    public class ExtraccionSubastaResponseDTO
    {
        public int TotalExtraidas { get; set; }
        public List<PrendaValidacionErrorDTO> ErroresValidacion { get; set; } = new();
        public List<ExtraccionSubastaItemDTO> PrendasExtraidas { get; set; } = new();
        public string Mensaje { get; set; }
        public bool TieneErrores => ErroresValidacion.Any();
    }
}
