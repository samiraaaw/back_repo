using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

public partial class Feriado
{
    [Key]
    public DateOnly Fecha { get; set; }

    [StringLength(150)]
    public string? Descripcion { get; set; }

    [Column("Es_Regional")]
    public bool? EsRegional { get; set; }
    public bool? Activo { get; set; }
}
