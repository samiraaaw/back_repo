using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

[Table("Log_Errores")]
public partial class LogErrore
{
    [Key]
    [Column("Log_ID")]
    public int LogId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaHora { get; set; }

    [StringLength(100)]
    public string? Procedimiento { get; set; }

    public string? MensajeError { get; set; }

    public string? ParametrosEntrada { get; set; }

    [StringLength(100)]
    public string? Usuario { get; set; }

    [StringLength(50)]
    public string? Origen { get; set; }
}
