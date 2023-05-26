using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Game.Simulation.Enemies;

interface IEnemyIcon
{
    Color IconColor { get; }
    SpriteDrawInfo<UVColorVertex, Color> MakeIconSprite(GameState game);
}
