using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Components;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Drawing
{
    class GhostBuildingRenderer<T> : BaseComponentRenderer<T>
        where T : IGameObject, IComponentOwner
    {
        private Shader shader = null!;

        protected override void OnAdded()
        {
            base.OnAdded();

            shader = Owner.Game.Meta.Blueprints.Shaders[ModAwareId.ForDefaultMod("building-ghost")];
        }

        public override void DrawSprite<TVertex, TVertexData>(
            SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 position, float size, float angle, TVertexData data)
        {
            var drawable =
                sprite.Sprite.MakeConcreteWith(Owner.Game.Meta.SpriteRenderers, sprite.Create, shader);

            drawable.Draw(position, size, angle, data);
        }
    }
}
