using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class TipoCuenta
{
    [Key]
    [Column("TipoCuenta_ID")]
    public int TipoCuentaId { get; set; }

    [Column("TipoCuenta_Descr")]
    [StringLength(35)]
    [Unicode(false)]
    public string? TipoCuentaDescr { get; set; }

    [Column("TipoCuenta_Act")]
    public bool? TipoCuentaAct { get; set; }

    [InverseProperty("TipoCuenta")]
    public virtual ICollection<CuentasBancaria> CuentasBancaria { get; set; } = new List<CuentasBancaria>();
}
