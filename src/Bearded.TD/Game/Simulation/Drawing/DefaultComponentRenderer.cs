using Bearded.Graphics.Vertices;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Drawing
{
    class DefaultComponentRenderer<T> : Component<T>, IComponentRenderer
        where T : IGameObject, IComponentOwner
    {
        protected override void OnAdded()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
            draw(Owner);
        }

        private void draw(IComponentOwner owner)
        {
            var components = owner.GetComponents<IComponent>();

            foreach (var component in components)
            {
                if (component is IDrawableComponent drawable)
                    drawable.Draw(this);

                if(component is INestedComponentOwner nested)
                    draw(nested.NestedComponentOwner);
            }
        }

        public void DrawSprite<TVertex, TVertexData>(
            SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 position, float size, float angle,
            TVertexData data) where TVertex : struct, IVertexData
        {
            var drawable =
                sprite.Sprite.MakeConcreteWith(Owner.Game.Meta.SpriteRenderers, sprite.Create, sprite.Shader);

            drawable.Draw(position, size, angle, data);
        }
    }
}
