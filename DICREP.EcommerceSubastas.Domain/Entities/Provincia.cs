using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class Provincia
{
    [Key]
    [Column("Provincia_ID")]
    public int ProvinciaId { get; set; }

    [Column("Provincia_CUT")]
    [StringLength(10)]
    [Unicode(false)]
    public string? ProvinciaCut { get; set; }

    [Column("Provincia_Nombre")]
    [StringLength(100)]
    [Unicode(false)]
    public string ProvinciaNombre { get; set; } = null!;

    [Column("Region_ID")]
    public int RegionId { get; set; }

    [Column("Provincia_Activo")]
    public bool? ProvinciaActivo { get; set; }

    [InverseProperty("Provincia")]
    public virtual ICollection<Comuna> Comunas { get; set; } = new List<Comuna>();

    [ForeignKey("RegionId")]
    [InverseProperty("Provincia")]
    public virtual Regione Region { get; set; } = null!;
}
