using Bearded.Graphics;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Game.Simulation.World;

interface IBiome : IBlueprint
{
    Color OverlayColor { get; }
    Material Material { get; }
}
