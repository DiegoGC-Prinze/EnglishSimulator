using System;
using System.Collections.Generic;

namespace EnglishSimulator.Models.Database;

public partial class Nivele
{
    public uint IdNivel { get; set; }

    public string NombreNivel { get; set; } = null!;

    public virtual ICollection<Pregunta> Pregunta { get; set; } = new List<Pregunta>();
}
