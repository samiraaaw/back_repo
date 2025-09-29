using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class Perfile
{
    [Key]
    [Column("Perfil_ID")]
    public int PerfilId { get; set; }

    [Column("Perfil_Nombre")]
    [StringLength(50)]
    [Unicode(false)]
    public string PerfilNombre { get; set; } = null!;

    [Column("Perfil_Activo")]
    public bool PerfilActivo { get; set; }

    [InverseProperty("Perfil")]
    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    [InverseProperty("Perfil")]
    public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
}
