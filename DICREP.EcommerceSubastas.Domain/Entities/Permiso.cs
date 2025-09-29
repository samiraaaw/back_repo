using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class Permiso
{
    [Key]
    [Column("Permiso_ID")]
    public int PermisoId { get; set; }

    [Column("Perfil_ID")]
    public int PerfilId { get; set; }

    [Column("Funcionalidad_ID")]
    public int FuncionalidadId { get; set; }

    public bool PuedeAcceder { get; set; }

    [ForeignKey("FuncionalidadId")]
    [InverseProperty("Permisos")]
    public virtual Funcionalidade Funcionalidad { get; set; } = null!;

    [ForeignKey("PerfilId")]
    [InverseProperty("Permisos")]
    public virtual Perfile Perfil { get; set; } = null!;
}
