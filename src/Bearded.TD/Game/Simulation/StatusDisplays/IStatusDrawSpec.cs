using Bearded.TD.Content.Mods;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

interface IStatusDrawSpec
{
    ModAwareSpriteId Icon { get; }
    double? Progress { get; }
}
