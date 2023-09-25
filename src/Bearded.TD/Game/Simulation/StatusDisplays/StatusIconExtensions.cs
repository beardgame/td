using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

static class StatusIconExtensions
{
    public static ISpriteBlueprint LoadStatusIconSprite(this Blueprints blueprints, string iconName) =>
        blueprints.Sprites[ModAwareId.ForDefaultMod("status-icons")].GetSprite(iconName);
}
