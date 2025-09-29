using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class Adjudicatario
{
    [Key]
    [Column("Adj_ID")]
    public int AdjId { get; set; }

    [Column("Adj_Rut")]
    public int? AdjRut { get; set; }

    [Column("Adj_RutDig")]
    [StringLength(1)]
    [Unicode(false)]
    public string? AdjRutDig { get; set; }

    [Column("Adj_Nombre")]
    [StringLength(20)]
    [Unicode(false)]
    public string? AdjNombre { get; set; }

    [Column("Adj_NombreS")]
    [StringLength(20)]
    [Unicode(false)]
    public string? AdjNombreS { get; set; }

    [Column("Adj_ApellidoP")]
    [StringLength(20)]
    [Unicode(false)]
    public string? AdjApellidoP { get; set; }

    [Column("Adj_ApellidoM")]
    [StringLength(20)]
    [Unicode(false)]
    public string? AdjApellidoM { get; set; }

    [Column("Adj_Correo")]
    [StringLength(255)]
    [Unicode(false)]
    public string? AdjCorreo { get; set; }

    [Column("Comuna_ID")]
    public int? ComunaId { get; set; }

    [ForeignKey("ComunaId")]
    [InverseProperty("Adjudicatarios")]
    public virtual Comuna? Comuna { get; set; }

    [InverseProperty("Adj")]
    public virtual ICollection<ResultadosAdjudicacion> ResultadosAdjudicacions { get; set; } = new List<ResultadosAdjudicacion>();
}
