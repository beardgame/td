using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

static class StatusIconExtensions
{
    private static readonly ModAwareId statusIconModAwareId = ModAwareId.ForDefaultMod("status-icons");

    public static ModAwareSpriteId ToStatusIconSpriteId(this string iconName) => new(statusIconModAwareId, iconName);

    public static ISpriteBlueprint LoadStatusIconSprite(this Blueprints blueprints, string iconName) =>
        blueprints.Sprites[statusIconModAwareId].GetSprite(iconName);
}
