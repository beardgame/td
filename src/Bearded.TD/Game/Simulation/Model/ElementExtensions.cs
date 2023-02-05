using System;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Damage;
using static Bearded.TD.Constants.Game.GameUI;

namespace Bearded.TD.Game.Simulation.Model;

static class ElementExtensions
{
    public static Color GetColor(this Element element) => element switch
    {
        Element.Dynamics => DynamicsColor,
        Element.Combustion => CombustionColor,
        Element.Conductivity => ConductivityColor,
        Element.Alchemy => AlchemyColor,
        Element.Hydrology => HydrologyColor,
        Element.Photonics => PhotonicsColor,
        _ => Color.Pink
    };
}
