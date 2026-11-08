//Librerias para el manejo de datos y validaciones
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishSimulator.Models
{
    public class RespuestaIntento
    {
        [Key]
        [Column("id_respuesta")]
        public int IdRespuesta { get; set; }

        [Column("id_intento")]
        [Required]
        public int IdIntento { get; set; }

        [Column("id_pregunta")]
        [Required]
        public int IdPregunta { get; set; }

        // El 'int?' permite el NULL en caso de que el usuario no haya seleccionado ninguna opción
        [Column("id_opcion")]
        public int? IdOpcion { get; set; }

        [Column("es_correcta")]
        public bool EsCorrecta { get; set; } = false;

        // 'int?' permite NULL, almacenando los segundos que tardó en contestar si es que lo hizo
        [Column("tiempo_respuesta")]
        public int? TiempoRespuesta { get; set; }

        // --- Propiedades de Navegación ---

        [ForeignKey("IdIntento")]
        public virtual IntentoExamen IntentoExamen { get; set; }

        [ForeignKey("IdPregunta")]
        public virtual Pregunta Pregunta { get; set; }

        [ForeignKey("IdOpcion")]
        public virtual Opcion Opcion { get; set; }
    }
}
