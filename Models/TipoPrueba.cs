//Librerias para el manejo de datos y validaciones
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EnglishSimulator.Models
{
    public class TipoPrueba
    {
        [Column("id_tipo_prueba")]
        public int IdTipoPrueba { get; set; }

        [Column("nombre_prueba")]
        [Required]
        [StringLength(50)]
        public string NombrePrueba{ get; set; }

        [Column("max_intentos")]
        public int MaxIntentos { get; set; }

        [Column("cantidad_preguntas")]
        public int CantidadPreguntas { get; set; }

    }
}
