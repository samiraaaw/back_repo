using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class Sucursale
{
    [Key]
    [Column("Sucursal_ID")]
    public int SucursalId { get; set; }

    [Column("Sucursal_Nombre")]
    [StringLength(100)]
    [Unicode(false)]
    public string SucursalNombre { get; set; } = null!;

    [Column("Sucursal_Direccion")]
    [StringLength(255)]
    [Unicode(false)]
    public string SucursalDireccion { get; set; } = null!;

    [Column("Sucursal_Activo")]
    public bool SucursalActivo { get; set; }

    [Column("Comuna_ID")]
    public int ComunaId { get; set; }

    [ForeignKey("ComunaId")]
    [InverseProperty("Sucursales")]
    public virtual Comuna Comuna { get; set; } = null!;

    [InverseProperty("Sucursal")]
    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
