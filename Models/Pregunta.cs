//Librerias para el manejo de datos y validaciones
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EnglishSimulator.Models
{
    public class Pregunta
    {
        [Key]
        [Column("id_pregunta")]
        public int IdPregunta { get; set; }

        [Column("id_nivel")]
        [Required]
        public int IdNivel { get; set; }

        [Column("texto_pregunta")]
        [Required]
        public string TextoPregunta { get; set; }

        [Column("segundos_tiempo")]
        public int SegundosTiempo { get; set; } = 60; // Asignamos el valor por defecto de 60 que pide tu script

        //PROPIEDAD DE NAVEGACION PARA LA RELACION CON NIVEL
        [ForeignKey("IdNivel")]
        public virtual Nivel Nivel { get; set; }
    }
}
