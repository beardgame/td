using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Drawing
{
    interface IComponentRenderer
    {
        void DrawSprite<TVertex, TVertexData>(
            SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 position, float size, float angle, TVertexData data)
            where TVertex : struct, IVertexData;
    }
}
