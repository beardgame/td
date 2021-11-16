using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;

namespace Bearded.TD.Game.Simulation.Drawing
{
    class GhostBuildingRenderer<T> : DefaultComponentRenderer<T>
        where T : IGameObject, IComponentOwner
    {
        private Shader shader = null!;

        protected override void OnAdded()
        {
            base.OnAdded();

            shader = Owner.Game.Meta.Blueprints.Shaders[ModAwareId.ForDefaultMod("building-ghost")];
        }

        protected override IDrawableSprite<TVertexData> Drawable<TVertex, TVertexData>(
            SpriteDrawInfo<TVertex, TVertexData> sprite)
        {
            return sprite.Sprite.MakeConcreteWith(Owner.Game.Meta.SpriteRenderers, sprite.Create, shader);
        }
    }
}
