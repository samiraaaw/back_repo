using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

[Table("Estados_Prendas")]
public partial class EstadosPrenda
{
    [Key]
    [Column("EstPrenda_ID")]
    public int EstPrendaId { get; set; }

    [Column("EstPrenda_Descr")]
    [StringLength(100)]
    [Unicode(false)]
    public string EstPrendaDescr { get; set; } = null!;

    [Column("EstPrenda_Activo")]
    public bool EstPrendaActivo { get; set; }

    [InverseProperty("EstPrenda")]
    public virtual ICollection<Clprenda> Clprenda { get; set; } = new List<Clprenda>();

    [InverseProperty("EstPrenda")]
    public virtual ICollection<HistorialPrenda> HistorialPrenda { get; set; } = new List<HistorialPrenda>();
}
