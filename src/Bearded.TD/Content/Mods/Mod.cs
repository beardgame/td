using System.Collections.Generic;
using Bearded.TD.Audio;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Models.Fonts;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.World;

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
