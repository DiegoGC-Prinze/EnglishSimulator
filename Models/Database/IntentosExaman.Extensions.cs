
namespace EnglishSimulator.Models.Database;

public partial class IntentosExaman
{
    public ushort SegundosRestantes { get; set; } = 60;

    public DateTime? UltimaSincronizacion { get; set; }
}
