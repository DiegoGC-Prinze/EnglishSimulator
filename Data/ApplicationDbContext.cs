using Microsoft.EntityFrameworkCore;
using EnglishSimulator.Models; 

namespace EnglishSimulator.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para manipular las tablas
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Nivel> Niveles { get; set; }
        public DbSet<TipoPrueba> TiposPruebas { get; set; }
        public DbSet<Pregunta> Preguntas { get; set; }
        public DbSet<Opcion> Opciones { get; set; }
        public DbSet<IntentoExamen> IntentosExamen { get; set; }
        public DbSet<RespuestaIntento> RespuestasIntento { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MAPPING DE LAS ENTIDADES A LAS TABLAS DE LA BASE DE DATOS
            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<Nivel>().ToTable("niveles");
            modelBuilder.Entity<TipoPrueba>().ToTable("tipos_pruebas");
            modelBuilder.Entity<Pregunta>().ToTable("preguntas");
            modelBuilder.Entity<Opcion>().ToTable("opciones");
            modelBuilder.Entity<IntentoExamen>().ToTable("intentos_examen");
            modelBuilder.Entity<RespuestaIntento>().ToTable("respuestas_intento");

            // DEFINICIÓN DE LAS CLAVES PRIMARIAS
            modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
            modelBuilder.Entity<Nivel>().HasKey(n => n.IdNivel);
            modelBuilder.Entity<TipoPrueba>().HasKey(t => t.IdTipoPrueba);
            modelBuilder.Entity<Pregunta>().HasKey(p => p.IdPregunta);
            modelBuilder.Entity<Opcion>().HasKey(o => o.IdOpcion);
            modelBuilder.Entity<IntentoExamen>().HasKey(i => i.IdIntento);
            modelBuilder.Entity<RespuestaIntento>().HasKey(r => r.IdRespuesta);

            // RESPETAR LAS CONSTRAINTS DE UNICIDAD PARA CAMPOS ESPECÍFICOS
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo).IsUnique();

            modelBuilder.Entity<Nivel>()
                .HasIndex(n => n.NombreNivel).IsUnique();

            modelBuilder.Entity<TipoPrueba>()
                .HasIndex(t => t.NombrePrueba).IsUnique();

            modelBuilder.Entity<RespuestaIntento>()
                .HasIndex(r => new { r.IdIntento, r.IdPregunta }).IsUnique();
        }
    }
}
