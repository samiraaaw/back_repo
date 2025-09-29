using System;
using System.Collections.Generic;

namespace GradoCerrado.Domain.Models;

public partial class PreguntasGenerada
{
    public int Id { get; set; }

    public int ModalidadId { get; set; }

    public int TemaId { get; set; }

    public string TextoPregunta { get; set; } = null!;

    public bool? RespuestaCorrectaBoolean { get; set; }

    public char? RespuestaCorrectaOpcion { get; set; }

    public string? RespuestaModelo { get; set; }

    public string? Explicacion { get; set; }

    public bool? Activa { get; set; }

    public string? CreadaPor { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public int? VecesUtilizada { get; set; }

    public int? VecesCorrecta { get; set; }

    public DateTime? UltimoUso { get; set; }

    public decimal? TasaAcierto { get; set; }

    public int? PromptSistemaId { get; set; }

    public string? ContextoFragmentos { get; set; }

    public string? ModeloIa { get; set; }

    public decimal? CalidadEstimada { get; set; }

    public virtual ModalidadTest Modalidad { get; set; } = null!;

    public virtual ICollection<PreguntaFragmentosQdrant> PreguntaFragmentosQdrants { get; set; } = new List<PreguntaFragmentosQdrant>();

    public virtual ICollection<PreguntaOpcione> PreguntaOpciones { get; set; } = new List<PreguntaOpcione>();

    public virtual PromptsSistema? PromptSistema { get; set; }

    public virtual Tema Tema { get; set; } = null!;

    public virtual ICollection<TestPregunta> TestPregunta { get; set; } = new List<TestPregunta>();
}
