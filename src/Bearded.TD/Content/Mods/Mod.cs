using System.Collections.Generic;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Content.Mods;

sealed class Mod(
    string id,
    string name,
    Blueprints blueprints,
    IDictionary<ModAwareId, UpgradeTag> tags)
{
    public string Id { get; } = id;
    public string Name { get; } = name;
    public Blueprints Blueprints { get; } = blueprints;
    public IDictionary<ModAwareId, UpgradeTag> Tags { get; } = tags;
}
