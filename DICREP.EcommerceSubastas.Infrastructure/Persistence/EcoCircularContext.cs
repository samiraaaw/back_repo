using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using DICREP.EcommerceSubastas.Domain.Entities;

namespace DICREP.EcommerceSubastas.Infrastructure.Persistence;

public partial class EcoCircularContext : DbContext
{
    public EcoCircularContext()
    {
    }

    public EcoCircularContext(DbContextOptions<EcoCircularContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Adjudicatario> Adjudicatarios { get; set; }

    public virtual DbSet<Auditoria> Auditorias { get; set; }

    public virtual DbSet<Banco> Bancos { get; set; }

    public virtual DbSet<Clprenda> Clprendas { get; set; }

    public virtual DbSet<Comuna> Comunas { get; set; }

    public virtual DbSet<ContactoOrganismo> ContactoOrganismos { get; set; }

    public virtual DbSet<CuentasBancaria> CuentasBancarias { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<EstadosBiene> EstadosBienes { get; set; }

    public virtual DbSet<EstadosPrenda> EstadosPrendas { get; set; }

    public virtual DbSet<Feriado> Feriados { get; set; }

    public virtual DbSet<Foto> Fotos { get; set; }

    public virtual DbSet<Funcionalidade> Funcionalidades { get; set; }

    public virtual DbSet<HistorialPrenda> HistorialPrendas { get; set; }

    public virtual DbSet<InformesTecnico> InformesTecnicos { get; set; }

    public virtual DbSet<LogErrore> LogErrores { get; set; }

    public virtual DbSet<Organismo> Organismos { get; set; }

    public virtual DbSet<Parametro> Parametros { get; set; }

    public virtual DbSet<Perfile> Perfiles { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Provincia> Provincias { get; set; }

    public virtual DbSet<Regione> Regiones { get; set; }

    public virtual DbSet<ResultadosAdjudicacion> ResultadosAdjudicacions { get; set; }

    public virtual DbSet<Sucursale> Sucursales { get; set; }

    public virtual DbSet<TipoAuditoria> TipoAuditorias { get; set; }

    public virtual DbSet<TipoCuenta> TipoCuentas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Adjudicatario>(entity =>
        {
            entity.HasKey(e => e.AdjId).HasName("PK__Adjudica__CDF53FF7DE585EED");

            entity.HasOne(d => d.Comuna).WithMany(p => p.Adjudicatarios).HasConstraintName("FK__Adjudicat__Comun__208CD6FA");
        });

        modelBuilder.Entity<Auditoria>(entity =>
        {
            entity.HasKey(e => e.AuditoriaId).HasName("PK__Auditori__D7259D328FE3F6C5");

            entity.Property(e => e.AuditoriaFecha).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Emp).WithMany(p => p.Auditoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auditoria_Empleado");

            entity.HasOne(d => d.TipoAuditoria).WithMany(p => p.Auditoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auditoria_TipoAuditoria");
        });

        modelBuilder.Entity<Banco>(entity =>
        {
            entity.HasKey(e => e.BancoId).HasName("PK__Bancos__8E9398834EC24FA7");

            entity.Property(e => e.BancoId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Clprenda>(entity =>
        {
            entity.HasKey(e => e.ClprendaId).HasName("PK__CLPrenda__14F9749F8EB78C7C");

            entity.HasOne(d => d.ContOrg).WithMany(p => p.Clprenda).HasConstraintName("FK_CLPrendas_ContactoOrganismos");

            entity.HasOne(d => d.Emp).WithMany(p => p.Clprenda).HasConstraintName("FK_CLPrendas_Empleado");

            entity.HasOne(d => d.EstBien).WithMany(p => p.Clprenda).HasConstraintName("FK_CLPrenda_EstadoBien");

            entity.HasOne(d => d.EstPrenda).WithMany(p => p.Clprenda).HasConstraintName("FK_CLPrendas_EstPrenda");

            entity.HasOne(d => d.Producto).WithMany(p => p.Clprenda).HasConstraintName("FK_CLPrendas_Productos");
        });

        modelBuilder.Entity<Comuna>(entity =>
        {
            entity.HasKey(e => e.ComunaId).HasName("PK__Comunas__228770D6605E268B");

            entity.HasOne(d => d.Provincia).WithMany(p => p.Comunas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comunas__Provinc__3C69FB99");
        });

        modelBuilder.Entity<ContactoOrganismo>(entity =>
        {
            entity.HasKey(e => e.ContOrgId).HasName("PK__Contacto__663B7A432330DD34");

            entity.HasOne(d => d.Comuna).WithMany(p => p.ContactoOrganismos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContactoOrganismos_Comuna");

            entity.HasOne(d => d.Organismo).WithMany(p => p.ContactoOrganismos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContactoOrganismos_Organismo");
        });

        modelBuilder.Entity<CuentasBancaria>(entity =>
        {
            entity.HasKey(e => e.CuentaId).HasName("PK__CuentasB__10E58735EB772790");

            entity.HasOne(d => d.Banco).WithMany(p => p.CuentasBancaria).HasConstraintName("FK__CuentasBa__Banco__1CBC4616");

            entity.HasOne(d => d.Organismo).WithMany(p => p.CuentasBancaria).HasConstraintName("FK__CuentasBa__Organ__1BC821DD");

            entity.HasOne(d => d.TipoCuenta).WithMany(p => p.CuentasBancaria).HasConstraintName("FK__CuentasBa__TipoC__1DB06A4F");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.EmpId).HasName("PK__Empleado__2623598B536E11F4");

            entity.HasOne(d => d.Perfil).WithMany(p => p.Empleados)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Empleados__Perfi__4F7CD00D");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Empleados)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Empleados__Sucur__5070F446");
        });

        modelBuilder.Entity<EstadosBiene>(entity =>
        {
            entity.HasKey(e => e.EstBienId).HasName("PK__EstadoBi__D610C09DD77A455D");

            entity.Property(e => e.EstBienId).ValueGeneratedNever();
        });

        modelBuilder.Entity<EstadosPrenda>(entity =>
        {
            entity.HasKey(e => e.EstPrendaId).HasName("PK__Estados___317C9EF34C921C3C");
        });

        modelBuilder.Entity<Feriado>(entity =>
        {
            entity.HasKey(e => e.Fecha).HasName("PK__FERIADOS__B30C8A5FBA9180B5");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.EsRegional).HasDefaultValue(false);
        });

        modelBuilder.Entity<Foto>(entity =>
        {
            entity.HasKey(e => e.FotoId).HasName("PK__Fotos__729CC97E66749A89");

            entity.HasOne(d => d.Clprenda).WithMany(p => p.Fotos).HasConstraintName("FK__Fotos__CLPrenda___6B24EA82");
        });

        modelBuilder.Entity<Funcionalidade>(entity =>
        {
            entity.HasKey(e => e.FuncionalidadId).HasName("PK__Funciona__FAAE541EE8E20E52");
        });

        modelBuilder.Entity<HistorialPrenda>(entity =>
        {
            entity.HasKey(e => e.HistPrendaId).HasName("PK__Historia__542F1957B902F406");

            entity.HasOne(d => d.Clprenda).WithMany(p => p.HistorialPrenda).HasConstraintName("FK__Historial__CLPre__656C112C");

            entity.HasOne(d => d.EstPrenda).WithMany(p => p.HistorialPrenda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Historial__EstPr__6477ECF3");
        });

        modelBuilder.Entity<InformesTecnico>(entity =>
        {
            entity.HasKey(e => e.InformeTecId).HasName("PK__Informes__BBA3A4361BBC8D5E");

            entity.HasOne(d => d.Clprenda).WithMany(p => p.InformesTecnicos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InformeTec_CLPrenda");
        });

        modelBuilder.Entity<LogErrore>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Log_Erro__2D26E7AEBF57F4DE");

            entity.Property(e => e.FechaHora).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Organismo>(entity =>
        {
            entity.HasKey(e => e.OrganismoId).HasName("PK__Organism__64621D142681B82C");

            entity.Property(e => e.OrganismoActiva).HasDefaultValue(true);
        });

        modelBuilder.Entity<Parametro>(entity =>
        {
            entity.HasKey(e => e.ParametroId).HasName("PK__Parametr__1064740371AF8836");

            entity.Property(e => e.ParametroFechaActualizacion).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Perfile>(entity =>
        {
            entity.HasKey(e => e.PerfilId).HasName("PK__Perfiles__CF2BD260089A5C91");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.PermisoId).HasName("PK__Permisos__985F9A5D6ED97613");

            entity.HasOne(d => d.Funcionalidad).WithMany(p => p.Permisos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permiso_Funcionalidad");

            entity.HasOne(d => d.Perfil).WithMany(p => p.Permisos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permiso_Perfil");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.ProductoId).HasName("PK__Producto__9F1B153D56318A35");

            entity.Property(e => e.ProductoId).ValueGeneratedNever();
            entity.Property(e => e.ProductoActivo).HasDefaultValue(true);
        });

        modelBuilder.Entity<Provincia>(entity =>
        {
            entity.HasKey(e => e.ProvinciaId).HasName("PK__Provinci__54AFE74F96BC56A1");

            entity.HasOne(d => d.Region).WithMany(p => p.Provincia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Provincia__Regio__398D8EEE");
        });

        modelBuilder.Entity<Regione>(entity =>
        {
            entity.HasKey(e => e.RegionId).HasName("PK__Regiones__A9EAD51F7D09CDE6");
        });

        modelBuilder.Entity<ResultadosAdjudicacion>(entity =>
        {
            entity.HasKey(e => e.ResultadoId).HasName("PK__Resultad__EFA17AFEC7CAA036");

            entity.HasOne(d => d.Adj).WithMany(p => p.ResultadosAdjudicacions).HasConstraintName("FK__Resultado__Adj_I__245D67DE");

            entity.HasOne(d => d.Clprenda).WithMany(p => p.ResultadosAdjudicacions).HasConstraintName("FK__Resultado__CLPre__236943A5");
        });

        modelBuilder.Entity<Sucursale>(entity =>
        {
            entity.HasKey(e => e.SucursalId).HasName("PK__Sucursal__372283B25CA04E9B");

            entity.HasOne(d => d.Comuna).WithMany(p => p.Sucursales)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sucursale__Comun__45F365D3");
        });

        modelBuilder.Entity<TipoAuditoria>(entity =>
        {
            entity.HasKey(e => e.TipoAuditoriaId).HasName("PK__Tipo_Aud__EE79756C6D4E0E63");

            entity.Property(e => e.TipoAuditoriaActivo).HasDefaultValue(true);
        });

        modelBuilder.Entity<TipoCuenta>(entity =>
        {
            entity.HasKey(e => e.TipoCuentaId).HasName("PK__TipoCuen__6A623B1E2D9FB224");

            entity.Property(e => e.TipoCuentaId).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
