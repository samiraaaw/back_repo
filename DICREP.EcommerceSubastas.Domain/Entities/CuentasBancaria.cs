using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class CuentasBancaria
{
    [Key]
    [Column("Cuenta_ID")]
    public int CuentaId { get; set; }

    [Column("Organismo_ID")]
    public int? OrganismoId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Rut { get; set; }

    [Column("Banco_ID")]
    public int? BancoId { get; set; }

    [Column("TipoCuenta_ID")]
    public int? TipoCuentaId { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string NumeroCuenta { get; set; } = null!;

    [StringLength(200)]
    [Unicode(false)]
    public string? NombreCuenta { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Correo { get; set; }

    [ForeignKey("BancoId")]
    [InverseProperty("CuentasBancaria")]
    public virtual Banco? Banco { get; set; }

    [ForeignKey("OrganismoId")]
    [InverseProperty("CuentasBancaria")]
    public virtual Organismo? Organismo { get; set; }

    [ForeignKey("TipoCuentaId")]
    [InverseProperty("CuentasBancaria")]
    public virtual TipoCuenta? TipoCuenta { get; set; }
}
