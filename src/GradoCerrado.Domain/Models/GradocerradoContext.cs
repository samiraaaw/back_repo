using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GradoCerrado.Domain.Models;

public partial class GradocerradoContext : DbContext
{
    public GradocerradoContext()
    {
    }

    public GradocerradoContext(DbContextOptions<GradocerradoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<Estudiante> Estudiantes { get; set; }

    public virtual DbSet<EstudianteNotificacionConfig> EstudianteNotificacionConfigs { get; set; }

    public virtual DbSet<FragmentosQdrant> FragmentosQdrants { get; set; }

    public virtual DbSet<MetricasEstudiante> MetricasEstudiantes { get; set; }

    public virtual DbSet<ModalidadTest> ModalidadTests { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<PreguntaFragmentosQdrant> PreguntaFragmentosQdrants { get; set; }

    public virtual DbSet<PreguntaOpcione> PreguntaOpciones { get; set; }

    public virtual DbSet<PreguntasGenerada> PreguntasGeneradas { get; set; }

    public virtual DbSet<PromptsSistema> PromptsSistemas { get; set; }

    public virtual DbSet<Tema> Temas { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<TestPregunta> TestPreguntas { get; set; }

    public virtual DbSet<TiposNotificacion> TiposNotificacions { get; set; }

    public virtual DbSet<TiposPrompt> TiposPrompts { get; set; }

    public virtual DbSet<TiposTest> TiposTests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=pg-gradocerrado.postgres.database.azure.com;Database=gradocerrado;Username=adminuser;Password=Derecho2024.;Port=5432;Trust Server Certificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("estado_test", new[] { "pendiente", "en_proceso", "dominado", "archivado" })
            .HasPostgresEnum("nivel_dificultad", new[] { "basico", "intermedio", "avanzado" })
            .HasPostgresEnum("prioridad_notificacion", new[] { "baja", "media", "alta", "urgente" })
            .HasPostgresEnum("tipo_pregunta", new[] { "verdadero_falso", "seleccion_multiple", "desarrollo" });

        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("areas_pkey");

            entity.ToTable("areas");

            entity.HasIndex(e => e.Nombre, "areas_nombre_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Icono)
                .HasMaxLength(50)
                .HasColumnName("icono");
            entity.Property(e => e.Importancia)
                .HasDefaultValueSql("0.5")
                .HasColumnName("importancia");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Estudiante>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("estudiantes_pkey");

            entity.ToTable("estudiantes");

            entity.HasIndex(e => e.Email, "estudiantes_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ApellidoMaterno)
                .HasMaxLength(100)
                .HasColumnName("apellido_materno");
            entity.Property(e => e.ApellidoPaterno)
                .HasMaxLength(100)
                .HasColumnName("apellido_paterno");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.FechaTestDiagnostico)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_test_diagnostico");
            entity.Property(e => e.FotoPerfil)
                .HasMaxLength(255)
                .HasColumnName("foto_perfil");
            entity.Property(e => e.IdAvatarSeleccionado)
                .HasMaxLength(50)
                .HasColumnName("id_avatar_seleccionado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(200)
                .HasComputedColumnSql("((((COALESCE(nombre, ''::character varying))::text ||\nCASE\n    WHEN ((segundo_nombre IS NOT NULL) AND ((segundo_nombre)::text <> ''::text)) THEN (' '::text || (segundo_nombre)::text)\n    ELSE ''::text\nEND) ||\nCASE\n    WHEN ((apellido_paterno IS NOT NULL) AND ((apellido_paterno)::text <> ''::text)) THEN (' '::text || (apellido_paterno)::text)\n    ELSE ''::text\nEND) ||\nCASE\n    WHEN ((apellido_materno IS NOT NULL) AND ((apellido_materno)::text <> ''::text)) THEN (' '::text || (apellido_materno)::text)\n    ELSE ''::text\nEND)", true)
                .HasColumnName("nombre_completo");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.SegundoNombre)
                .HasMaxLength(100)
                .HasColumnName("segundo_nombre");
            entity.Property(e => e.TestDiagnosticoCompletado)
                .HasDefaultValue(false)
                .HasColumnName("test_diagnostico_completado");
            entity.Property(e => e.UltimoAcceso)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ultimo_acceso");
            entity.Property(e => e.Verificado)
                .HasDefaultValue(false)
                .HasColumnName("verificado");
        });

        modelBuilder.Entity<EstudianteNotificacionConfig>(entity =>
        {
            entity.HasKey(e => e.EstudianteId).HasName("estudiante_notificacion_config_pkey");

            entity.ToTable("estudiante_notificacion_config");

            entity.Property(e => e.EstudianteId)
                .ValueGeneratedNever()
                .HasColumnName("estudiante_id");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.NotificacionesHabilitadas)
                .HasDefaultValue(true)
                .HasColumnName("notificaciones_habilitadas");
            entity.Property(e => e.TokenDispositivo)
                .HasMaxLength(255)
                .HasColumnName("token_dispositivo");

            entity.HasOne(d => d.Estudiante).WithOne(p => p.EstudianteNotificacionConfig)
                .HasForeignKey<EstudianteNotificacionConfig>(d => d.EstudianteId)
                .HasConstraintName("estudiante_notificacion_config_estudiante_id_fkey");
        });

        modelBuilder.Entity<FragmentosQdrant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fragmentos_qdrant_pkey");

            entity.ToTable("fragmentos_qdrant");

            entity.HasIndex(e => e.ChunkId, "fragmentos_qdrant_chunk_id_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.AreaId).HasColumnName("area_id");
            entity.Property(e => e.ChunkId)
                .HasMaxLength(200)
                .HasColumnName("chunk_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.TemaId).HasColumnName("tema_id");
            entity.Property(e => e.Title)
                .HasMaxLength(500)
                .HasColumnName("title");
            entity.Property(e => e.UsadoEnPreguntas)
                .HasDefaultValue(0)
                .HasColumnName("usado_en_preguntas");

            entity.HasOne(d => d.Area).WithMany(p => p.FragmentosQdrants)
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("fragmentos_qdrant_area_id_fkey");

            entity.HasOne(d => d.Tema).WithMany(p => p.FragmentosQdrants)
                .HasForeignKey(d => d.TemaId)
                .HasConstraintName("fragmentos_qdrant_tema_id_fkey");
        });

        modelBuilder.Entity<MetricasEstudiante>(entity =>
        {
            entity.HasKey(e => e.EstudianteId).HasName("metricas_estudiante_pkey");

            entity.ToTable("metricas_estudiante");

            entity.Property(e => e.EstudianteId)
                .ValueGeneratedNever()
                .HasColumnName("estudiante_id");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.PrimeraFechaEstudio).HasColumnName("primera_fecha_estudio");
            entity.Property(e => e.PromedioAciertos)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("promedio_aciertos");
            entity.Property(e => e.PromedioPreguntasDia)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("promedio_preguntas_dia");
            entity.Property(e => e.RachaDiasActual)
                .HasDefaultValue(0)
                .HasColumnName("racha_dias_actual");
            entity.Property(e => e.RachaDiasMaxima)
                .HasDefaultValue(0)
                .HasColumnName("racha_dias_maxima");
            entity.Property(e => e.TotalDiasEstudiados)
                .HasDefaultValue(0)
                .HasColumnName("total_dias_estudiados");
            entity.Property(e => e.UltimoDiaEstudio).HasColumnName("ultimo_dia_estudio");
            entity.Property(e => e.VersionCalculo)
                .HasDefaultValue(1)
                .HasColumnName("version_calculo");
        });

        modelBuilder.Entity<ModalidadTest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("modalidad_test_pkey");

            entity.ToTable("modalidad_test");

            entity.HasIndex(e => e.Nombre, "modalidad_test_nombre_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Notificacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notificaciones_pkey");

            entity.ToTable("notificaciones");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccionTomada)
                .HasDefaultValue(false)
                .HasColumnName("accion_tomada");
            entity.Property(e => e.DatosAdicionales)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("datos_adicionales");
            entity.Property(e => e.Enviado)
                .HasDefaultValue(false)
                .HasColumnName("enviado");
            entity.Property(e => e.EstudianteId).HasColumnName("estudiante_id");
            entity.Property(e => e.FechaAccion)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_accion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaEnviado)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_enviado");
            entity.Property(e => e.FechaLeido)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_leido");
            entity.Property(e => e.FechaProgramada)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_programada");
            entity.Property(e => e.Leido)
                .HasDefaultValue(false)
                .HasColumnName("leido");
            entity.Property(e => e.Mensaje).HasColumnName("mensaje");
            entity.Property(e => e.TipoNotificacionId).HasColumnName("tipo_notificacion_id");
            entity.Property(e => e.Titulo)
                .HasMaxLength(200)
                .HasColumnName("titulo");

            entity.HasOne(d => d.Estudiante).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.EstudianteId)
                .HasConstraintName("notificaciones_estudiante_id_fkey");

            entity.HasOne(d => d.TipoNotificacion).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.TipoNotificacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("notificaciones_tipo_notificacion_id_fkey");
        });

        modelBuilder.Entity<PreguntaFragmentosQdrant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pregunta_fragmentos_qdrant_pkey");

            entity.ToTable("pregunta_fragmentos_qdrant");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChunkId)
                .HasMaxLength(200)
                .HasColumnName("chunk_id");
            entity.Property(e => e.OrdenUso)
                .HasDefaultValue((short)1)
                .HasColumnName("orden_uso");
            entity.Property(e => e.PreguntaGeneradaId).HasColumnName("pregunta_generada_id");
            entity.Property(e => e.Relevancia)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("1.0")
                .HasColumnName("relevancia");

            entity.HasOne(d => d.PreguntaGenerada).WithMany(p => p.PreguntaFragmentosQdrants)
                .HasForeignKey(d => d.PreguntaGeneradaId)
                .HasConstraintName("pregunta_fragmentos_qdrant_pregunta_generada_id_fkey");
        });

        modelBuilder.Entity<PreguntaOpcione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pregunta_opciones_pkey");

            entity.ToTable("pregunta_opciones");

            entity.HasIndex(e => new { e.PreguntaGeneradaId, e.Opcion }, "pregunta_opciones_pregunta_generada_id_opcion_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EsCorrecta)
                .HasDefaultValue(false)
                .HasColumnName("es_correcta");
            entity.Property(e => e.Opcion)
                .HasMaxLength(1)
                .HasColumnName("opcion");
            entity.Property(e => e.PreguntaGeneradaId).HasColumnName("pregunta_generada_id");
            entity.Property(e => e.TextoOpcion).HasColumnName("texto_opcion");

            entity.HasOne(d => d.PreguntaGenerada).WithMany(p => p.PreguntaOpciones)
                .HasForeignKey(d => d.PreguntaGeneradaId)
                .HasConstraintName("pregunta_opciones_pregunta_generada_id_fkey");
        });

        modelBuilder.Entity<PreguntasGenerada>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("preguntas_generadas_pkey");

            entity.ToTable("preguntas_generadas");

            entity.HasIndex(e => e.TemaId, "idx_preguntas_tema");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.CalidadEstimada)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("0.8")
                .HasColumnName("calidad_estimada");
            entity.Property(e => e.ContextoFragmentos).HasColumnName("contexto_fragmentos");
            entity.Property(e => e.CreadaPor)
                .HasMaxLength(100)
                .HasDefaultValueSql("'sistema'::character varying")
                .HasColumnName("creada_por");
            entity.Property(e => e.Explicacion).HasColumnName("explicacion");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.ModalidadId).HasColumnName("modalidad_id");
            entity.Property(e => e.ModeloIa)
                .HasMaxLength(100)
                .HasDefaultValueSql("'gpt-4'::character varying")
                .HasColumnName("modelo_ia");
            entity.Property(e => e.PromptSistemaId).HasColumnName("prompt_sistema_id");
            entity.Property(e => e.RespuestaCorrectaBoolean).HasColumnName("respuesta_correcta_boolean");
            entity.Property(e => e.RespuestaCorrectaOpcion)
                .HasMaxLength(1)
                .HasColumnName("respuesta_correcta_opcion");
            entity.Property(e => e.RespuestaModelo).HasColumnName("respuesta_modelo");
            entity.Property(e => e.TasaAcierto)
                .HasPrecision(4, 3)
                .HasComputedColumnSql("\nCASE\n    WHEN (veces_utilizada > 0) THEN round(((veces_correcta)::numeric / (veces_utilizada)::numeric), 3)\n    ELSE (0)::numeric\nEND", true)
                .HasColumnName("tasa_acierto");
            entity.Property(e => e.TemaId).HasColumnName("tema_id");
            entity.Property(e => e.TextoPregunta).HasColumnName("texto_pregunta");
            entity.Property(e => e.UltimoUso)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ultimo_uso");
            entity.Property(e => e.VecesCorrecta)
                .HasDefaultValue(0)
                .HasColumnName("veces_correcta");
            entity.Property(e => e.VecesUtilizada)
                .HasDefaultValue(0)
                .HasColumnName("veces_utilizada");

            entity.HasOne(d => d.Modalidad).WithMany(p => p.PreguntasGenerada)
                .HasForeignKey(d => d.ModalidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("preguntas_generadas_modalidad_id_fkey");

            entity.HasOne(d => d.PromptSistema).WithMany(p => p.PreguntasGenerada)
                .HasForeignKey(d => d.PromptSistemaId)
                .HasConstraintName("preguntas_generadas_prompt_sistema_id_fkey");

            entity.HasOne(d => d.Tema).WithMany(p => p.PreguntasGenerada)
                .HasForeignKey(d => d.TemaId)
                .HasConstraintName("preguntas_generadas_tema_id_fkey");
        });

        modelBuilder.Entity<PromptsSistema>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prompts_sistema_pkey");

            entity.ToTable("prompts_sistema");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Plantilla).HasColumnName("plantilla");
            entity.Property(e => e.TipoPromptId).HasColumnName("tipo_prompt_id");
            entity.Property(e => e.UsuarioActualizacion)
                .HasMaxLength(100)
                .HasColumnName("usuario_actualizacion");
            entity.Property(e => e.UsuarioCreacion)
                .HasMaxLength(100)
                .HasDefaultValueSql("'sistema'::character varying")
                .HasColumnName("usuario_creacion");
            entity.Property(e => e.Version)
                .HasDefaultValue(1)
                .HasColumnName("version");

            entity.HasOne(d => d.TipoPrompt).WithMany(p => p.PromptsSistemas)
                .HasForeignKey(d => d.TipoPromptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("prompts_sistema_tipo_prompt_id_fkey");
        });

        modelBuilder.Entity<Tema>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("temas_pkey");

            entity.ToTable("temas");

            entity.HasIndex(e => e.AreaId, "idx_temas_area");

            entity.HasIndex(e => new { e.Nombre, e.AreaId }, "unique_tema_area").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.AreaId).HasColumnName("area_id");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.NombreNorm).HasColumnName("nombre_norm");

            entity.HasOne(d => d.Area).WithMany(p => p.Temas)
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("temas_area_id_fkey");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tests_pkey");

            entity.ToTable("tests");

            entity.HasIndex(e => e.EstudianteId, "idx_tests_estudiante");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AreaId).HasColumnName("area_id");
            entity.Property(e => e.Completado)
                .HasDefaultValue(false)
                .HasColumnName("completado");
            entity.Property(e => e.DuracionEstimada).HasColumnName("duracion_estimada");
            entity.Property(e => e.DuracionSegundos).HasColumnName("duracion_segundos");
            entity.Property(e => e.EstudianteId).HasColumnName("estudiante_id");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.HoraFin)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("hora_fin");
            entity.Property(e => e.HoraInicio)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("hora_inicio");
            entity.Property(e => e.ModalidadId).HasColumnName("modalidad_id");
            entity.Property(e => e.NotasAdicionales).HasColumnName("notas_adicionales");
            entity.Property(e => e.NumeroPreguntasAvanzado)
                .HasDefaultValue(0)
                .HasColumnName("numero_preguntas_avanzado");
            entity.Property(e => e.NumeroPreguntasBasico)
                .HasDefaultValue(0)
                .HasColumnName("numero_preguntas_basico");
            entity.Property(e => e.NumeroPreguntasIntermedio)
                .HasDefaultValue(0)
                .HasColumnName("numero_preguntas_intermedio");
            entity.Property(e => e.NumeroPreguntasTotal).HasColumnName("numero_preguntas_total");
            entity.Property(e => e.PorcentajeAcierto)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("porcentaje_acierto");
            entity.Property(e => e.PuntajeMaximo)
                .HasPrecision(5, 2)
                .HasColumnName("puntaje_maximo");
            entity.Property(e => e.PuntajeObtenido)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("puntaje_obtenido");
            entity.Property(e => e.TiempoLimiteMinutos).HasColumnName("tiempo_limite_minutos");
            entity.Property(e => e.TipoTestId).HasColumnName("tipo_test_id");

            entity.HasOne(d => d.Area).WithMany(p => p.Tests)
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("tests_area_id_fkey");

            entity.HasOne(d => d.Estudiante).WithMany(p => p.Tests)
                .HasForeignKey(d => d.EstudianteId)
                .HasConstraintName("tests_estudiante_id_fkey");

            entity.HasOne(d => d.Modalidad).WithMany(p => p.Tests)
                .HasForeignKey(d => d.ModalidadId)
                .HasConstraintName("tests_modalidad_id_fkey");

            entity.HasOne(d => d.TipoTest).WithMany(p => p.Tests)
                .HasForeignKey(d => d.TipoTestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tests_tipo_test_id_fkey");
        });

        modelBuilder.Entity<TestPregunta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("test_preguntas_pkey");

            entity.ToTable("test_preguntas");

            entity.HasIndex(e => e.TestId, "idx_test_preguntas_test");

            entity.HasIndex(e => new { e.TestId, e.NumeroOrden }, "test_preguntas_test_id_numero_orden_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EsCorrecta).HasColumnName("es_correcta");
            entity.Property(e => e.FechaRespuesta)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_respuesta");
            entity.Property(e => e.NumeroOrden).HasColumnName("numero_orden");
            entity.Property(e => e.PreguntaGeneradaId).HasColumnName("pregunta_generada_id");
            entity.Property(e => e.RespuestaBoolean).HasColumnName("respuesta_boolean");
            entity.Property(e => e.RespuestaOpcion)
                .HasMaxLength(1)
                .HasColumnName("respuesta_opcion");
            entity.Property(e => e.RespuestaTexto).HasColumnName("respuesta_texto");
            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.TiempoRespuestaSegundos).HasColumnName("tiempo_respuesta_segundos");

            entity.HasOne(d => d.PreguntaGenerada).WithMany(p => p.TestPregunta)
                .HasForeignKey(d => d.PreguntaGeneradaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("test_preguntas_pregunta_generada_id_fkey");

            entity.HasOne(d => d.Test).WithMany(p => p.TestPregunta)
                .HasForeignKey(d => d.TestId)
                .HasConstraintName("test_preguntas_test_id_fkey");
        });

        modelBuilder.Entity<TiposNotificacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tipos_notificacion_pkey");

            entity.ToTable("tipos_notificacion");

            entity.HasIndex(e => e.Nombre, "tipos_notificacion_nombre_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<TiposPrompt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tipos_prompt_pkey");

            entity.ToTable("tipos_prompt");

            entity.HasIndex(e => e.Nombre, "tipos_prompt_nombre_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<TiposTest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tipos_test_pkey");

            entity.ToTable("tipos_test");

            entity.HasIndex(e => e.Nombre, "tipos_test_nombre_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
