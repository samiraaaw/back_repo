using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

[Index("ParametroCodigo", Name = "UQ__Parametr__5F6A8C954A1118C8", IsUnique = true)]
public partial class Parametro
{
    [Key]
    [Column("Parametro_ID")]
    public int ParametroId { get; set; }

    [Column("Parametro_Codigo")]
    [StringLength(50)]
    [Unicode(false)]
    public string ParametroCodigo { get; set; } = null!;

    [Column("Parametro_Valor")]
    public int ParametroValor { get; set; }

    [Column("Parametro_Descripcion")]
    [StringLength(200)]
    [Unicode(false)]
    public string? ParametroDescripcion { get; set; }

    [Column("Parametro_FechaActualizacion", TypeName = "datetime")]
    public DateTime? ParametroFechaActualizacion { get; set; }
}
