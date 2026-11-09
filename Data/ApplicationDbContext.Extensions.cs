using EnglishSimulator.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace EnglishSimulator.Data;

public partial class ApplicationDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IntentosExaman>(entity =>
        {
            entity.Property(e => e.SegundosRestantes)
                .HasColumnName("segundos_restantes")
                .HasColumnType("smallint unsigned")
                .HasDefaultValue((ushort)60);

            entity.Property(e => e.UltimaSincronizacion)
                .HasColumnName("ultima_sincronizacion")
                .HasColumnType("datetime(6)");
        });

        modelBuilder.Entity<RespuestasIntento>(entity =>
        {
            entity.Property(e => e.OrdenPregunta)
                .HasColumnName("orden_pregunta")
                .HasColumnType("tinyint unsigned");

            entity.HasIndex(
                e => new { e.IdIntento, e.OrdenPregunta },
                "uq_intento_orden")
                .IsUnique();
        });
    }
}
