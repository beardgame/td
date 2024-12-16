using Bearded.TD.Content.Mods;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

readonly record struct StatusAppearance(ModAwareSpriteId Icon, bool Enabled, double? Progress)
{
    public static StatusAppearance IconOnly(ModAwareSpriteId icon) => new(icon, true, null);
    public static StatusAppearance IconAndProgress(ModAwareSpriteId icon, double progress) => new(icon, true, progress);

    public StatusAppearance Disabled() => this with { Enabled = false };
}
