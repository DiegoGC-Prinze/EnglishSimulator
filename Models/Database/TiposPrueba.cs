using System;
using System.Collections.Generic;

namespace EnglishSimulator.Models.Database;

public partial class TiposPrueba
{
    public uint IdTipoPrueba { get; set; }

    public string NombrePrueba { get; set; } = null!;

    public byte MaxIntentos { get; set; }

    public byte CantidadPreguntas { get; set; }

    public virtual ICollection<IntentosExaman> IntentosExamen { get; set; } = new List<IntentosExaman>();
}
