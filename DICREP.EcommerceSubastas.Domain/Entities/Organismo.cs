using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

[Index("OrganismoCodigo", Name = "UQ_Organismos_Codigo", IsUnique = true)]
[Index("OrganismoCodigo", Name = "UQ__Organism__DC086841F4AF96C0", IsUnique = true)]
public partial class Organismo
{
    [Key]
    [Column("Organismo_ID")]
    public int OrganismoId { get; set; }

    [Column("Organismo_Nombre")]
    [StringLength(250)]
    [Unicode(false)]
    public string OrganismoNombre { get; set; } = null!;

    [Column("Organismo_Rut")]
    [StringLength(20)]
    [Unicode(false)]
    public string? OrganismoRut { get; set; }

    [Column("Organismo_Codigo")]
    public int? OrganismoCodigo { get; set; }

    [Column("Organismo_Activa")]
    public bool OrganismoActiva { get; set; }

    [InverseProperty("Organismo")]
    public virtual ICollection<ContactoOrganismo> ContactoOrganismos { get; set; } = new List<ContactoOrganismo>();

    [InverseProperty("Organismo")]
    public virtual ICollection<CuentasBancaria> CuentasBancaria { get; set; } = new List<CuentasBancaria>();
}
