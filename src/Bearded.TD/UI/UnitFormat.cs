using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.Damage;

namespace Bearded.TD.UI;

static class UnitFormat
{
    public static string ToUiString(this Experience xp) => ((int) xp.NumericValue).ToString();
    public static string ToUiString(this HitPoints hp) => ((int) hp.NumericValue).ToString();
}
