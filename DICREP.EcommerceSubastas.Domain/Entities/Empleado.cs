using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class Empleado
{
    [Key]
    [Column("Emp_ID")]
    public int EmpId { get; set; }

    [Column("Emp_Usuario")]
    [StringLength(50)]
    [Unicode(false)]
    public string EmpUsuario { get; set; } = null!;

    [Column("Emp_Rut")]
    public int EmpRut { get; set; }

    [Column("Emp_RutDig")]
    [StringLength(1)]
    [Unicode(false)]
    public string EmpRutDig { get; set; } = null!;

    [Column("Emp_Nombre")]
    [StringLength(20)]
    [Unicode(false)]
    public string EmpNombre { get; set; } = null!;

    [Column("Emp_SegundoNombre")]
    [StringLength(20)]
    [Unicode(false)]
    public string? EmpSegundoNombre { get; set; }

    [Column("Emp_Apellido")]
    [StringLength(20)]
    [Unicode(false)]
    public string EmpApellido { get; set; } = null!;

    [Column("Emp_SegundoApellido")]
    [StringLength(20)]
    [Unicode(false)]
    public string? EmpSegundoApellido { get; set; }

    [Column("Emp_Anexo")]
    [StringLength(20)]
    [Unicode(false)]
    public string? EmpAnexo { get; set; }

    [Column("Emp_Correo")]
    [StringLength(100)]
    [Unicode(false)]
    public string EmpCorreo { get; set; } = null!;

    [Column("Emp_Activo")]
    public bool EmpActivo { get; set; }

    [Column("Emp_FechaLog", TypeName = "datetime")]
    public DateTime? EmpFechaLog { get; set; }

    [Column("Emp_FechaExp", TypeName = "datetime")]
    public DateTime? EmpFechaExp { get; set; }

    [Column("Perfil_ID")]
    public int PerfilId { get; set; }

    [Column("Sucursal_ID")]
    public int SucursalId { get; set; }

    public int AuthMethod { get; set; }

    [StringLength(256)]
    public string? PasswordHash { get; set; }

    [StringLength(100)]
    public string? ClaveUnicaSub { get; set; }

    [InverseProperty("Emp")]
    public virtual ICollection<Auditoria> Auditoria { get; set; } = new List<Auditoria>();

    [InverseProperty("Emp")]
    public virtual ICollection<Clprenda> Clprenda { get; set; } = new List<Clprenda>();

    [ForeignKey("PerfilId")]
    [InverseProperty("Empleados")]
    public virtual Perfile Perfil { get; set; } = null!;

    [ForeignKey("SucursalId")]
    [InverseProperty("Empleados")]
    public virtual Sucursale Sucursal { get; set; } = null!;
}
