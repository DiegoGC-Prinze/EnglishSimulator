using System;
using System.Collections.Generic;

namespace EnglishSimulator.Models.Database;

public partial class RespuestasIntento
{
    public uint IdRespuesta { get; set; }

    public uint IdIntento { get; set; }

    public uint IdPregunta { get; set; }

    /// <summary>
    /// NULL = sin respuesta o tiempo agotado
    /// </summary>
    public uint? IdOpcion { get; set; }

    public bool EsCorrecta { get; set; }

    /// <summary>
    /// Segundos transcurridos para responder
    /// </summary>
    public ushort? TiempoRespuesta { get; set; }

    public virtual IntentosExaman IdIntentoNavigation { get; set; } = null!;

    public virtual Opcione? IdOpcionNavigation { get; set; }

    public virtual Pregunta IdPreguntaNavigation { get; set; } = null!;
}
