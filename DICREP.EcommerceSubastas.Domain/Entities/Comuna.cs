using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class Comuna
{
    [Key]
    [Column("Comuna_ID")]
    public int ComunaId { get; set; }

    [Column("Comuna_CUT")]
    [StringLength(15)]
    [Unicode(false)]
    public string? ComunaCut { get; set; }

    [Column("Comuna_Nombre")]
    [StringLength(100)]
    [Unicode(false)]
    public string ComunaNombre { get; set; } = null!;

    [Column("Provincia_ID")]
    public int ProvinciaId { get; set; }

    [Column("Comuna_Activo")]
    public bool ComunaActivo { get; set; }

    [InverseProperty("Comuna")]
    public virtual ICollection<Adjudicatario> Adjudicatarios { get; set; } = new List<Adjudicatario>();

    [InverseProperty("Comuna")]
    public virtual ICollection<ContactoOrganismo> ContactoOrganismos { get; set; } = new List<ContactoOrganismo>();

    [ForeignKey("ProvinciaId")]
    [InverseProperty("Comunas")]
    public virtual Provincia Provincia { get; set; } = null!;

    [InverseProperty("Comuna")]
    public virtual ICollection<Sucursale> Sucursales { get; set; } = new List<Sucursale>();
}
