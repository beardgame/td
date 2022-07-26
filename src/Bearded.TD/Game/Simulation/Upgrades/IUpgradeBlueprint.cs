using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Upgrades;

interface IUpgradeBlueprint : IBlueprint
{
    string Name { get; }
    ResourceAmount Cost { get; }
    ImmutableArray<IUpgradeEffect> Effects { get; }
}
