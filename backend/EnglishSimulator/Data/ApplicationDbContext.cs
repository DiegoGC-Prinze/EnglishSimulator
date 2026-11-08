using System;
using System.Collections.Generic;
using EnglishSimulator.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace EnglishSimulator.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<IntentosExaman> IntentosExamen { get; set; }

    public virtual DbSet<Nivele> Niveles { get; set; }

    public virtual DbSet<Opcione> Opciones { get; set; }

    public virtual DbSet<Pregunta> Preguntas { get; set; }

    public virtual DbSet<RespuestasIntento> RespuestasIntentos { get; set; }

    public virtual DbSet<TiposPrueba> TiposPruebas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<IntentosExaman>(entity =>
        {
            entity.HasKey(e => e.IdIntento).HasName("PRIMARY");

            entity.ToTable("intentos_examen");

            entity.HasIndex(e => e.IdTipoPrueba, "fk_intentos_tipo");

            entity.HasIndex(e => new { e.IdUsuario, e.IdTipoPrueba }, "idx_intentos_usuario_tipo");

            entity.Property(e => e.IdIntento).HasColumnName("id_intento");
            entity.Property(e => e.FechaIntento)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_intento");
            entity.Property(e => e.IdTipoPrueba).HasColumnName("id_tipo_prueba");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.PorcentajeObtenido)
                .HasPrecision(5, 2)
                .HasColumnName("porcentaje_obtenido");
            entity.Property(e => e.PuntajeRequerido)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("'70.00'")
                .HasColumnName("puntaje_requerido");
            entity.Property(e => e.Resultado)
                .HasMaxLength(20)
                .HasDefaultValueSql("'EN_PROCESO'")
                .HasColumnName("resultado");

            entity.HasOne(d => d.IdTipoPruebaNavigation).WithMany(p => p.IntentosExamen)
                .HasForeignKey(d => d.IdTipoPrueba)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_intentos_tipo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.IntentosExamen)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_intentos_usuarios");
        });

        modelBuilder.Entity<Nivele>(entity =>
        {
            entity.HasKey(e => e.IdNivel).HasName("PRIMARY");

            entity.ToTable("niveles");

            entity.HasIndex(e => e.NombreNivel, "nombre_nivel").IsUnique();

            entity.Property(e => e.IdNivel).HasColumnName("id_nivel");
            entity.Property(e => e.NombreNivel)
                .HasMaxLength(40)
                .HasColumnName("nombre_nivel");
        });

        modelBuilder.Entity<Opcione>(entity =>
        {
            entity.HasKey(e => e.IdOpcion).HasName("PRIMARY");

            entity.ToTable("opciones");

            entity.HasIndex(e => e.IdPregunta, "idx_opciones_pregunta");

            entity.Property(e => e.IdOpcion).HasColumnName("id_opcion");
            entity.Property(e => e.EsCorrecta).HasColumnName("es_correcta");
            entity.Property(e => e.IdPregunta).HasColumnName("id_pregunta");
            entity.Property(e => e.TextoOpcion)
                .HasMaxLength(255)
                .HasColumnName("texto_opcion");

            entity.HasOne(d => d.IdPreguntaNavigation).WithMany(p => p.Opciones)
                .HasForeignKey(d => d.IdPregunta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_opciones_preguntas");
        });

        modelBuilder.Entity<Pregunta>(entity =>
        {
            entity.HasKey(e => e.IdPregunta).HasName("PRIMARY");

            entity.ToTable("preguntas");

            entity.HasIndex(e => e.IdNivel, "idx_preguntas_nivel");

            entity.Property(e => e.IdPregunta).HasColumnName("id_pregunta");
            entity.Property(e => e.IdNivel).HasColumnName("id_nivel");
            entity.Property(e => e.SegundosTiempo)
                .HasDefaultValueSql("'60'")
                .HasColumnName("segundos_tiempo");
            entity.Property(e => e.TextoPregunta)
                .HasColumnType("text")
                .HasColumnName("texto_pregunta");

            entity.HasOne(d => d.IdNivelNavigation).WithMany(p => p.Pregunta)
                .HasForeignKey(d => d.IdNivel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_preguntas_niveles");
        });

        modelBuilder.Entity<RespuestasIntento>(entity =>
        {
            entity.HasKey(e => e.IdRespuesta).HasName("PRIMARY");

            entity.ToTable("respuestas_intento");

            entity.HasIndex(e => e.IdOpcion, "idx_respuestas_opcion");

            entity.HasIndex(e => e.IdPregunta, "idx_respuestas_pregunta");

            entity.HasIndex(e => new { e.IdIntento, e.IdPregunta }, "uq_respuesta_por_pregunta").IsUnique();

            entity.Property(e => e.IdRespuesta).HasColumnName("id_respuesta");
            entity.Property(e => e.EsCorrecta).HasColumnName("es_correcta");
            entity.Property(e => e.IdIntento).HasColumnName("id_intento");
            entity.Property(e => e.IdOpcion)
                .HasComment("NULL = sin respuesta o tiempo agotado")
                .HasColumnName("id_opcion");
            entity.Property(e => e.IdPregunta).HasColumnName("id_pregunta");
            entity.Property(e => e.TiempoRespuesta)
                .HasComment("Segundos transcurridos para responder")
                .HasColumnName("tiempo_respuesta");

            entity.HasOne(d => d.IdIntentoNavigation).WithMany(p => p.RespuestasIntentos)
                .HasForeignKey(d => d.IdIntento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_respuestas_intentos");

            entity.HasOne(d => d.IdOpcionNavigation).WithMany(p => p.RespuestasIntentos)
                .HasForeignKey(d => d.IdOpcion)
                .HasConstraintName("fk_respuestas_opciones");

            entity.HasOne(d => d.IdPreguntaNavigation).WithMany(p => p.RespuestasIntentos)
                .HasForeignKey(d => d.IdPregunta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_respuestas_preguntas");
        });

        modelBuilder.Entity<TiposPrueba>(entity =>
        {
            entity.HasKey(e => e.IdTipoPrueba).HasName("PRIMARY");

            entity.ToTable("tipos_pruebas");

            entity.HasIndex(e => e.NombrePrueba, "nombre_prueba").IsUnique();

            entity.Property(e => e.IdTipoPrueba).HasColumnName("id_tipo_prueba");
            entity.Property(e => e.CantidadPreguntas).HasColumnName("cantidad_preguntas");
            entity.Property(e => e.MaxIntentos).HasColumnName("max_intentos");
            entity.Property(e => e.NombrePrueba)
                .HasMaxLength(50)
                .HasColumnName("nombre_prueba");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Correo, "correo").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .HasComment("Almacenar hash de contrasena, nunca texto plano")
                .HasColumnName("contrasena");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
