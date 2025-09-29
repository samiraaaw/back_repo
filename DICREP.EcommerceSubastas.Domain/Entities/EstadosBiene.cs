using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class EstadosBiene
{
    [Key]
    [Column("EstBien_ID")]
    public int EstBienId { get; set; }

    [Column("EstBien_Descr")]
    [StringLength(30)]
    [Unicode(false)]
    public string? EstBienDescr { get; set; }

    [Column("EstBien_Activo")]
    public bool? EstBienActivo { get; set; }

    [InverseProperty("EstBien")]
    public virtual ICollection<Clprenda> Clprenda { get; set; } = new List<Clprenda>();
}
