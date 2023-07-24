using Bearded.Graphics;

namespace Bearded.TD.Game.Simulation.World;

interface IBiome : IBlueprint
{
    Color OverlayColor { get; }
}
