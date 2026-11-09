using System;
using System.Collections.Generic;

namespace EnglishSimulator.Models.Database;

public partial class Opcione
{
    public uint IdOpcion { get; set; }

    public uint IdPregunta { get; set; }

    public string TextoOpcion { get; set; } = null!;

    public bool EsCorrecta { get; set; }

    public virtual Pregunta IdPreguntaNavigation { get; set; } = null!;

    public virtual ICollection<RespuestasIntento> RespuestasIntentos { get; set; } = new List<RespuestasIntento>();
}
