using System;
using System.Collections.Generic;

namespace EnglishSimulator.Models.Database;

public partial class Usuario
{
    public uint IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    /// <summary>
    /// Almacenar hash de contrasena, nunca texto plano
    /// </summary>
    public string Contrasena { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<IntentosExaman> IntentosExamen { get; set; } = new List<IntentosExaman>();
}
