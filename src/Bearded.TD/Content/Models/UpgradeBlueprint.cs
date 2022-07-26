using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Content.Models;

sealed record UpgradeBlueprint(
    ModAwareId Id,
    string Name,
    ResourceAmount Cost,
    ImmutableArray<IUpgradeEffect> Effects) : IUpgradeBlueprint;
