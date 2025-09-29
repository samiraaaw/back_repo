using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class ContactoOrganismo
{
    [Key]
    [Column("ContOrg_ID")]
    public int ContOrgId { get; set; }

    [Column("Organismo_ID")]
    public int OrganismoId { get; set; }

    [Column("ContOrg_NombreP")]
    [StringLength(40)]
    [Unicode(false)]
    public string? ContOrgNombreP { get; set; }

    [Column("ContOrg_NombreS")]
    [StringLength(40)]
    [Unicode(false)]
    public string? ContOrgNombreS { get; set; }

    [Column("ContOrg_ApellidoP")]
    [StringLength(40)]
    [Unicode(false)]
    public string? ContOrgApellidoP { get; set; }

    [Column("ContOrg_ApellidoM")]
    [StringLength(40)]
    [Unicode(false)]
    public string? ContOrgApellidoM { get; set; }

    [Column("ContOrg_Cargo")]
    [StringLength(20)]
    [Unicode(false)]
    public string? ContOrgCargo { get; set; }

    [Column("ContOrg_Direccion")]
    [StringLength(255)]
    [Unicode(false)]
    public string? ContOrgDireccion { get; set; }

    [Column("ContOrg_Correo")]
    [StringLength(255)]
    [Unicode(false)]
    public string? ContOrgCorreo { get; set; }

    [Column("ContOrg_Telefono")]
    [StringLength(50)]
    [Unicode(false)]
    public string? ContOrgTelefono { get; set; }

    [Column("Comuna_ID")]
    public int ComunaId { get; set; }

    [InverseProperty("ContOrg")]
    public virtual ICollection<Clprenda> Clprenda { get; set; } = new List<Clprenda>();

    [ForeignKey("ComunaId")]
    [InverseProperty("ContactoOrganismos")]
    public virtual Comuna Comuna { get; set; } = null!;

    [ForeignKey("OrganismoId")]
    [InverseProperty("ContactoOrganismos")]
    public virtual Organismo Organismo { get; set; } = null!;
}
