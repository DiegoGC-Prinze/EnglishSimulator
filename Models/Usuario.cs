//Librerias para el manejo de datos y validaciones
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishSimulator.Models
{
    public class Usuario
    {
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("nombre")]
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Column("correo")]
        [Required]
        [StringLength(150)]
        public string Correo { get; set; }

        [Column("contrasena")]
        [Required]
        public string Contrasena { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; }
    }
}
