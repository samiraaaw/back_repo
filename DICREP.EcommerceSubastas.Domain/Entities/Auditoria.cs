using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class Auditoria
{
    [Key]
    [Column("Auditoria_ID")]
    public int AuditoriaId { get; set; }

    [Column("Auditoria_Accion")]
    [StringLength(100)]
    public string AuditoriaAccion { get; set; } = null!;

    [Column("Auditoria_Tabla")]
    [StringLength(100)]
    public string AuditoriaTabla { get; set; } = null!;

    [Column("Auditoria_Campo")]
    [StringLength(100)]
    public string AuditoriaCampo { get; set; } = null!;

    [Column("Auditoria_Fecha", TypeName = "datetime")]
    public DateTime AuditoriaFecha { get; set; }

    [Column("Auditoria_ValorAnt")]
    [StringLength(250)]
    public string? AuditoriaValorAnt { get; set; }

    [Column("Auditoria_ValorAct")]
    [StringLength(250)]
    public string? AuditoriaValorAct { get; set; }

    [Column("Auditoria_PC")]
    [StringLength(50)]
    public string? AuditoriaPc { get; set; }

    [Column("Emp_ID")]
    public int EmpId { get; set; }

    [Column("TipoAuditoria_ID")]
    public int TipoAuditoriaId { get; set; }

    [Column("Auditoria_RegistroID")]
    public long? AuditoriaRegistroId { get; set; }

    [ForeignKey("EmpId")]
    [InverseProperty("Auditoria")]
    public virtual Empleado Emp { get; set; } = null!;

    [ForeignKey("TipoAuditoriaId")]
    [InverseProperty("Auditoria")]
    public virtual TipoAuditoria TipoAuditoria { get; set; } = null!;
}
