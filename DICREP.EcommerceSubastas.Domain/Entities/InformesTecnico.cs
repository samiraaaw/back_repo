using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class InformesTecnico
{
    [Key]
    [Column("InformeTec_ID")]
    public int InformeTecId { get; set; }

    [Column("InformeTec_Url")]
    [StringLength(255)]
    [Unicode(false)]
    public string? InformeTecUrl { get; set; }

    [Column("InformeTec_Tipo")]
    [StringLength(10)]
    [Unicode(false)]
    public string? InformeTecTipo { get; set; }

    [Column("CLPrenda_ID")]
    public long ClprendaId { get; set; }

    [ForeignKey("ClprendaId")]
    [InverseProperty("InformesTecnicos")]
    public virtual Clprenda Clprenda { get; set; } = null!;
}
