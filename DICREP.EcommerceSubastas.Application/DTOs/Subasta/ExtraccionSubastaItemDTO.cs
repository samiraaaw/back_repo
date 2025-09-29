using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.DTOs.Subasta
{
    public class ExtraccionSubastaItemDTO
    {
        public string CLPrendaCodigo { get; set; }
        public DateTime? FechaIngresoFase2 { get; set; }
        public DateTime? FechaTerminoFase2 { get; set; }
        public string Categoria { get; set; }
        public string NombreProducto { get; set; }
        public string Descripcion { get; set; }
        public string EstadoBien { get; set; }
        public int? Cantidad { get; set; }
        public decimal? PrecioTotal { get; set; }
        public decimal? Comision { get; set; }
        public decimal? Incremento { get; set; }

        // Datos del organismo
        public string NombreOrganizacion { get; set; }
        public string RutOrganizacion { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Comuna { get; set; }
        public string DireccionContacto { get; set; }
        public string Region { get; set; }

        // Fotos (hasta 6)
        public string? Foto1 { get; set; }
        public string? Foto2 { get; set; }
        public string? Foto3 { get; set; }
        public string? Foto4 { get; set; }
        public string? Foto5 { get; set; }
        public string? Foto6 { get; set; }

        // Informes técnicos (hasta 6)
        public string? Informe1 { get; set; }
        public string? Informe2 { get; set; }
        public string? Informe3 { get; set; }
        public string? Informe4 { get; set; }
        public string? Informe5 { get; set; }
        public string? Informe6 { get; set; }
    }
}