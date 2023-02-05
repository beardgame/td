using System;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Model;

namespace Bearded.TD.Game.Simulation.Technologies;

static class TechnologyBranchExtensions
{
    public static Element ToElement(this TechnologyBranch branch) => branch switch
    {
        TechnologyBranch.Dynamics => Element.Dynamics,
        TechnologyBranch.Combustion => Element.Combustion,
        TechnologyBranch.Conductivity => Element.Conductivity,
        TechnologyBranch.Photonics => Element.Photonics,
        TechnologyBranch.Hydrology => Element.Hydrology,
        TechnologyBranch.Alchemy => Element.Alchemy,
        _ => throw new ArgumentOutOfRangeException(nameof(branch), branch, null)
    };

    public static TechnologyBranch ToTechnologyBranch(this Element element) => element switch
    {
        Element.Dynamics => TechnologyBranch.Dynamics,
        Element.Combustion => TechnologyBranch.Combustion,
        Element.Conductivity => TechnologyBranch.Conductivity,
        Element.Photonics => TechnologyBranch.Photonics,
        Element.Hydrology => TechnologyBranch.Hydrology,
        Element.Alchemy => TechnologyBranch.Alchemy,
        _ => throw new ArgumentOutOfRangeException(nameof(element), element, null)
    };

    public static Color GetColor(this TechnologyBranch branch) => branch.ToElement().GetColor();
}
