using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class Banco
{
    [Key]
    [Column("Banco_ID")]
    public int BancoId { get; set; }

    [Column("Banco_SBIF")]
    [StringLength(10)]
    [Unicode(false)]
    public string? BancoSbif { get; set; }

    [Column("Banco_Nombre")]
    [StringLength(25)]
    [Unicode(false)]
    public string? BancoNombre { get; set; }

    [Column("Banco_Activo")]
    public bool? BancoActivo { get; set; }

    [InverseProperty("Banco")]
    public virtual ICollection<CuentasBancaria> CuentasBancaria { get; set; } = new List<CuentasBancaria>();
}
