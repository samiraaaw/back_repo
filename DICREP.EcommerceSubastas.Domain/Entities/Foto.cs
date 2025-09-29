using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class Foto
{
    [Key]
    [Column("Foto_ID")]
    public int FotoId { get; set; }

    [Column("Foto_Url")]
    [StringLength(255)]
    [Unicode(false)]
    public string FotoUrl { get; set; } = null!;

    [Column("Foto_Formato")]
    [StringLength(10)]
    [Unicode(false)]
    public string? FotoFormato { get; set; }

    [Column("Foto_FechaRecep", TypeName = "datetime")]
    public DateTime? FotoFechaRecep { get; set; }

    [Column("CLPrenda_ID")]
    public long? ClprendaId { get; set; }

    [ForeignKey("ClprendaId")]
    [InverseProperty("Fotos")]
    public virtual Clprenda? Clprenda { get; set; }
}
