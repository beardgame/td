using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

interface IArcEmissionParameters
{
    IGameObjectBlueprint Arc { get; }

    [Modifiable(1, Type = AttributeType.ArcBranches)]
    public int Branches { get; }

    [Modifiable(1, Type = AttributeType.ArcBounces)]
    public int Bounces { get; }

    [Modifiable(2)]
    public Unit MaxBounceDistance { get; }
}
