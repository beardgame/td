using Bearded.TD.Content.Mods;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

readonly record struct StatusAppearance(ModAwareSpriteId Icon, double? Progress)
{
    public static StatusAppearance IconOnly(ModAwareSpriteId icon) => new(icon, null);
    public static StatusAppearance IconAndProgress(ModAwareSpriteId icon, double progress) => new(icon, progress);
}
