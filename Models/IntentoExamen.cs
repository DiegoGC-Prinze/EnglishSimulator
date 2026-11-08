//Librerias para el manejo de datos y validaciones
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishSimulator.Models
{
    public class IntentoExamen
    {
        [Key]
        [Column("id_intento")]
        public int IdIntento { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("id_tipo_prueba")]
        [Required]
        public int IdTipoPrueba { get; set; }

        // Usamos decimal? (con el signo de interrogación) porque en MySQL permites valores NULL
        [Column("porcentaje_obtenido")]
        public decimal? PorcentajeObtenido { get; set; }

        [Column("puntaje_requerido")]
        public decimal PuntajeRequerido { get; set; } = 70.00m; // Valor por defecto igual al de MySQL

        [Column("resultado")]
        [Required]
        [StringLength(20)]
        public string Resultado { get; set; } = "EN_PROCESO"; // Valor por defecto igual al de MySQL

        [Column("fecha_intento")]
        public DateTime FechaIntento { get; set; } = DateTime.Now;

        // --- Propiedades de Navegación ---

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("IdTipoPrueba")]
        public virtual TipoPrueba TipoPrueba { get; set; }
    }
}
