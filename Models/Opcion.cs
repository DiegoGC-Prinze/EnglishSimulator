//Librerias para el manejo de datos y validaciones
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EnglishSimulator.Models
{
    public class Opcion
    {
        [Key]
        [Column("id_opcion")]
        public int IdOpcion { get; set; }

        [Column("id_pregunta")]
        [Required]
        public int IdPregunta { get; set; }

        [Column("texto_opcion")]
        [Required]
        [StringLength(255)]
        public string TextoOpcion { get; set; }

        [Column("es_correcta")]
        public bool EsCorrecta { get; set; }

        // Propiedad de navegación para relacionar con la tabla Preguntas
        [ForeignKey("IdPregunta")]
        public virtual Pregunta Pregunta { get; set; }
    }
}
