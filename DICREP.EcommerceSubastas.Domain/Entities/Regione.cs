using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class Regione
{
    [Key]
    [Column("Region_ID")]
    public int RegionId { get; set; }

    [Column("Region_CUT")]
    [StringLength(10)]
    [Unicode(false)]
    public string? RegionCut { get; set; }

    [Column("Region_Nombre")]
    [StringLength(100)]
    [Unicode(false)]
    public string RegionNombre { get; set; } = null!;

    [Column("Region_Numero")]
    [StringLength(10)]
    [Unicode(false)]
    public string RegionNumero { get; set; } = null!;

    [Column("Region_Abreviatura")]
    [StringLength(12)]
    [Unicode(false)]
    public string RegionAbreviatura { get; set; } = null!;

    [Column("Region_Activo")]
    public bool RegionActivo { get; set; }

    [InverseProperty("Region")]
    public virtual ICollection<Provincia> Provincia { get; set; } = new List<Provincia>();
}
