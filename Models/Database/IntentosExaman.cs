using System;
using System.Collections.Generic;

namespace EnglishSimulator.Models.Database;

public partial class IntentosExaman
{
    public uint IdIntento { get; set; }

    public uint IdUsuario { get; set; }

    public uint IdTipoPrueba { get; set; }

    public decimal? PorcentajeObtenido { get; set; }

    public decimal PuntajeRequerido { get; set; }

    public string Resultado { get; set; } = null!;

    public DateTime FechaIntento { get; set; }

    public virtual TiposPrueba IdTipoPruebaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<RespuestasIntento> RespuestasIntentos { get; set; } = new List<RespuestasIntento>();
}
