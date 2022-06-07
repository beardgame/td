using System;
using Bearded.Graphics;

namespace Bearded.TD.Game.Simulation.Technologies;

static class TechnologyBranchColors
{
    public static readonly Color Dynamics = Color.Gray;
    public static readonly Color Combustion = Color.OrangeRed;
    public static readonly Color Conductivity = Color.Purple;
    public static readonly Color Alchemy = Color.LimeGreen;
    public static readonly Color Hydrology = Color.LightBlue;
    public static readonly Color Photonics = Color.Yellow;

    public static Color GetColor(this TechnologyBranch branch) => branch switch
    {
        TechnologyBranch.Dynamics => Dynamics,
        TechnologyBranch.Combustion => Combustion,
        TechnologyBranch.Conductivity => Conductivity,
        TechnologyBranch.Alchemy => Alchemy,
        TechnologyBranch.Hydrology => Hydrology,
        TechnologyBranch.Photonics => Photonics,
        _ => throw new ArgumentOutOfRangeException(nameof(branch), branch, null)
    };
}
