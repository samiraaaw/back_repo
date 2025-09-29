using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

[Index("Codigo", Name = "UQ_Funcionalidad_Codigo", IsUnique = true)]
public partial class Funcionalidade
{
    [Key]
    [Column("Funcionalidad_ID")]
    public int FuncionalidadId { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column("Endpoint_api")]
    [StringLength(200)]
    public string EndpointApi { get; set; } = null!;

    [Column("MetodoHTTP")]
    [StringLength(10)]
    public string MetodoHttp { get; set; } = null!;

    [StringLength(100)]
    public string? Grupo { get; set; }

    public bool EsMenu { get; set; }

    public bool Activo { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Codigo { get; set; } = null!;

    [InverseProperty("Funcionalidad")]
    public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
}
