using Bearded.Graphics;
using static Bearded.TD.Constants.Game.GameUI;

namespace Bearded.TD.Game.Simulation.Model;

static class ElementExtensions
{
    public static Color GetColor(this Element element) => element switch
    {
        Element.Force => ForceColor,
        Element.Fire => FireColor,
        Element.Lightning => LightningColor,
        Element.Alchemy => AlchemyColor,
        Element.Water => WaterColor,
        Element.Energy => EnergyColor,
        _ => Color.Pink
    };
}
