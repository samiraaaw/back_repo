using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class Producto
{
    [Key]
    [Column("Producto_ID")]
    public int ProductoId { get; set; }

    [Column("Producto_Nombre")]
    [StringLength(155)]
    [Unicode(false)]
    public string ProductoNombre { get; set; } = null!;

    [Column("Producto_Activo")]
    public bool ProductoActivo { get; set; }

    [InverseProperty("Producto")]
    public virtual ICollection<Clprenda> Clprenda { get; set; } = new List<Clprenda>();
}
