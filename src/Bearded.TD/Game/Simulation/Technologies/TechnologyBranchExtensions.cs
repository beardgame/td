using System;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Model;

namespace Bearded.TD.Game.Simulation.Technologies;

static class TechnologyBranchExtensions
{
    public static Element ToElement(this TechnologyBranch branch) => branch switch
    {
        TechnologyBranch.Kinetics => Element.Kinetics,
        TechnologyBranch.Fire => Element.Fire,
        TechnologyBranch.Lightning => Element.Lightning,
        TechnologyBranch.Energy => Element.Energy,
        TechnologyBranch.Water => Element.Water,
        TechnologyBranch.Alchemy => Element.Alchemy,
        _ => throw new ArgumentOutOfRangeException(nameof(branch), branch, null)
    };

    public static TechnologyBranch ToTechnologyBranch(this Element element) => element switch
    {
        Element.Kinetics => TechnologyBranch.Kinetics,
        Element.Fire => TechnologyBranch.Fire,
        Element.Lightning => TechnologyBranch.Lightning,
        Element.Energy => TechnologyBranch.Energy,
        Element.Water => TechnologyBranch.Water,
        Element.Alchemy => TechnologyBranch.Alchemy,
        _ => throw new ArgumentOutOfRangeException(nameof(element), element, null)
    };

    public static Color GetColor(this TechnologyBranch branch) => branch.ToElement().GetColor();
}
