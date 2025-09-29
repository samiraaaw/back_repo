using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

[Table("Historial_Prendas")]
public partial class HistorialPrenda
{
    [Key]
    [Column("histPrenda_ID")]
    public int HistPrendaId { get; set; }

    [Column("histPrenda_FechCam", TypeName = "datetime")]
    public DateTime HistPrendaFechCam { get; set; }

    [Column("histPrenda_Ant")]
    [StringLength(100)]
    [Unicode(false)]
    public string? HistPrendaAnt { get; set; }

    [Column("EstPrenda_ID")]
    public int EstPrendaId { get; set; }

    [Column("CLPrenda_ID")]
    public long? ClprendaId { get; set; }

    [ForeignKey("ClprendaId")]
    [InverseProperty("HistorialPrenda")]
    public virtual Clprenda? Clprenda { get; set; }

    [ForeignKey("EstPrendaId")]
    [InverseProperty("HistorialPrenda")]
    public virtual EstadosPrenda EstPrenda { get; set; } = null!;
}
