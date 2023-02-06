using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Content.Models;

sealed record Module(
    ModAwareId Id,
    Element AffinityElement,
    SocketShape SocketShape,
    ImmutableArray<IUpgradeEffect> Effects) : IModule;
