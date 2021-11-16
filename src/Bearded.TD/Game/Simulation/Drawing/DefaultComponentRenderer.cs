using Bearded.TD.Game.Simulation.Components;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Drawing
{
    class DefaultComponentRenderer<T> : BaseComponentRenderer<T>
        where T : IGameObject, IComponentOwner
    {
        public override void DrawSprite<TVertex, TVertexData>(
            SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 position, float size, float angle, TVertexData data)
        {
            var drawable =
                sprite.Sprite.MakeConcreteWith(Owner.Game.Meta.SpriteRenderers, sprite.Create, sprite.Shader);

            drawable.Draw(position, size, angle, data);
        }
    }
}
