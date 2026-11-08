//Librerias para el manejo de datos y validaciones
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishSimulator.Models
{
    public class Nivel
    {
        [Key]
        [Column("id_nivel")]
        public int IdNivel { get; set; }

        [Column("nombre_nivel")]
        [Required]
        [StringLength(50)]
        public string NombreNivel { get; set; }
    }
}
