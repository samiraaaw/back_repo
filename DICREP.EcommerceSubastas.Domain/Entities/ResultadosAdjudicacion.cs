using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

[Table("ResultadosAdjudicacion")]
public partial class ResultadosAdjudicacion
{
    [Key]
    [Column("Resultado_ID")]
    public long ResultadoId { get; set; }

    [Column("CLPrenda_ID")]
    public long? ClprendaId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MontoMinimo { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MontoAdjudicacion { get; set; }

    [Column("totalAdjudicacion", TypeName = "decimal(18, 2)")]
    public decimal? TotalAdjudicacion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaAdjudicacion { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Moneda { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? ComisionPorc { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalComision { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? IvaComision { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalRecaudar { get; set; }

    [Column("Adj_ID")]
    public int? AdjId { get; set; }

    [ForeignKey("AdjId")]
    [InverseProperty("ResultadosAdjudicacions")]
    public virtual Adjudicatario? Adj { get; set; }

    [ForeignKey("ClprendaId")]
    [InverseProperty("ResultadosAdjudicacions")]
    public virtual Clprenda? Clprenda { get; set; }
}
