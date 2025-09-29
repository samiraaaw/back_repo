using System;
using System.Collections.Generic;

namespace GradoCerrado.Domain.Models;

public partial class EstudianteNotificacionConfig
{
    public int EstudianteId { get; set; }

    public bool? NotificacionesHabilitadas { get; set; }

    public string? TokenDispositivo { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual Estudiante Estudiante { get; set; } = null!;
}
