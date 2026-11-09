using System;
using System.Collections.Generic;

namespace EnglishSimulator.Models.Database;

public partial class Pregunta
{
    public uint IdPregunta { get; set; }

    public uint IdNivel { get; set; }

    public string TextoPregunta { get; set; } = null!;

    public ushort SegundosTiempo { get; set; }

    public virtual Nivele IdNivelNavigation { get; set; } = null!;

    public virtual ICollection<Opcione> Opciones { get; set; } = new List<Opcione>();

    public virtual ICollection<RespuestasIntento> RespuestasIntentos { get; set; } = new List<RespuestasIntento>();
}
