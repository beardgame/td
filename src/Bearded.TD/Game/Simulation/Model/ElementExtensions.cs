using System;
using System.Collections.Generic;
using Bearded.Graphics;
using static Bearded.TD.Constants.Game.GameUI;

namespace Bearded.TD.Game.Simulation.Model;

static class ElementExtensions
{
    public static IEnumerable<Element> Enumerate() => Enum.GetValues<Element>();

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
