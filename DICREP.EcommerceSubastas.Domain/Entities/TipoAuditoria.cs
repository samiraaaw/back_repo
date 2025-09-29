using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

[Table("Tipo_Auditorias")]
public partial class TipoAuditoria
{
    [Key]
    [Column("TipoAuditoria_ID")]
    public int TipoAuditoriaId { get; set; }

    [Column("TipoAuditoria_Descr")]
    [StringLength(100)]
    public string TipoAuditoriaDescr { get; set; } = null!;

    [Column("TipoAuditoria_Activo")]
    public bool TipoAuditoriaActivo { get; set; }

    [InverseProperty("TipoAuditoria")]
    public virtual ICollection<Auditoria> Auditoria { get; set; } = new List<Auditoria>();
}
