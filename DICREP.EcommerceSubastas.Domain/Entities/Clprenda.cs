using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Domain.Entities;

[Table("CLPrendas")]
[Index("ClprendaCod", Name = "UQ_CLPrendaCod", IsUnique = true)]
[Index("ClprendaCod", Name = "UQ__CLPrenda__7969E56AC39CD1C8", IsUnique = true)]
public partial class Clprenda
{
    [Key]
    [Column("CLPrenda_ID")]
    public long ClprendaId { get; set; }

    [Column("CLPrenda_Cod")]
    [StringLength(64)]
    [Unicode(false)]
    public string ClprendaCod { get; set; } = null!;

    [Column("CLPrenda_Nombre")]
    [StringLength(255)]
    [Unicode(false)]
    public string ClprendaNombre { get; set; } = null!;

    [Column("CLPrenda_Descr")]
    [Unicode(false)]
    public string ClprendaDescr { get; set; } = null!;

    [Column("CLPrenda_Cant")]
    public int? ClprendaCant { get; set; }

    [Column("CLPrenda_ValorUni", TypeName = "decimal(10, 2)")]
    public decimal? ClprendaValorUni { get; set; }

    [Column("CLPrenda_ValorTotal", TypeName = "decimal(10, 2)")]
    public decimal? ClprendaValorTotal { get; set; }

    [Column("CLPrenda_Tamano")]
    [StringLength(255)]
    [Unicode(false)]
    public string? ClprendaTamano { get; set; }

    [Column("CLPrenda_Color")]
    [StringLength(255)]
    [Unicode(false)]
    public string? ClprendaColor { get; set; }

    [Column("CLPrenda_FechaCrea", TypeName = "datetime")]
    public DateTime? ClprendaFechaCrea { get; set; }

    [Column("CLPrenda_FechaPub", TypeName = "datetime")]
    public DateTime? ClprendaFechaPub { get; set; }

    [Column("CLPrenda_FechaTerm", TypeName = "datetime")]
    public DateTime? ClprendaFechaTerm { get; set; }

    [Column("Emp_ID")]
    public int? EmpId { get; set; }

    [Column("Producto_ID")]
    public int? ProductoId { get; set; }

    [Column("EstPrenda_ID")]
    public int? EstPrendaId { get; set; }

    [Column("EstBien_ID")]
    public int? EstBienId { get; set; }

    [Column("ContOrg_ID")]
    public int? ContOrgId { get; set; }

    [ForeignKey("ContOrgId")]
    [InverseProperty("Clprenda")]
    public virtual ContactoOrganismo? ContOrg { get; set; }

    [ForeignKey("EmpId")]
    [InverseProperty("Clprenda")]
    public virtual Empleado? Emp { get; set; }

    [ForeignKey("EstBienId")]
    [InverseProperty("Clprenda")]
    public virtual EstadosBiene? EstBien { get; set; }

    [ForeignKey("EstPrendaId")]
    [InverseProperty("Clprenda")]
    public virtual EstadosPrenda? EstPrenda { get; set; }

    [InverseProperty("Clprenda")]
    public virtual ICollection<Foto> Fotos { get; set; } = new List<Foto>();

    [InverseProperty("Clprenda")]
    public virtual ICollection<HistorialPrenda> HistorialPrenda { get; set; } = new List<HistorialPrenda>();

    [InverseProperty("Clprenda")]
    public virtual ICollection<InformesTecnico> InformesTecnicos { get; set; } = new List<InformesTecnico>();

    [ForeignKey("ProductoId")]
    [InverseProperty("Clprenda")]
    public virtual Producto? Producto { get; set; }

    [InverseProperty("Clprenda")]
    public virtual ICollection<ResultadosAdjudicacion> ResultadosAdjudicacions { get; set; } = new List<ResultadosAdjudicacion>();
}
