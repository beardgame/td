using Bearded.Graphics.Vertices;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Drawing
{
    class DefaultComponentRenderer<T> : Component<T>, IComponentRenderer
        where T : IGameObject, IComponentOwner
    {
        private IVisibility? visibility;

        protected override void OnAdded()
        {
            ComponentDependencies.Depend<IVisibility>(Owner, Events, v => visibility = v);
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
            if (visibility?.Visibility.IsVisible() ?? true)
            {
                drawRecursively(Owner);
            }
        }

        private void drawRecursively(IComponentOwner owner)
        {
            var components = owner.GetComponents<IComponent>();

            foreach (var component in components)
            {
                if (component is IDrawableComponent drawable)
                    drawable.Draw(this);

                if (component is INestedComponentOwner nested)
                    drawRecursively(nested.NestedComponentOwner);
            }
        }

        public void DrawSprite<TVertex, TVertexData>(
            SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 position, float size, float angle, TVertexData data)
            where TVertex : struct, IVertexData
        {
            Drawable(sprite).Draw(position, size, angle, data);
        }

        public void DrawQuad<TVertex, TVertexData>(
            SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight,
            Vector3 bottomLeft, TVertexData data)
            where TVertex : struct, IVertexData
        {
            Drawable(sprite).DrawQuad(topLeft, topRight, bottomRight, bottomLeft, data);
        }

        public void DrawQuad<TVertex, TVertexData>(
            SpriteDrawInfo<TVertex, TVertexData> sprite,
            Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv0,
            Vector2 uv1, Vector2 uv2, Vector2 uv3, TVertexData data)
            where TVertex : struct, IVertexData
        {
            Drawable(sprite).DrawQuad(p0, p1, p2, p3, uv0, uv1, uv2, uv3, data);
        }

        protected virtual IDrawableSprite<TVertexData> Drawable<TVertex, TVertexData>(
            SpriteDrawInfo<TVertex, TVertexData> sprite)
            where TVertex : struct, IVertexData
        {
            return sprite.Sprite.MakeConcreteWith(Owner.Game.Meta.SpriteRenderers, sprite.Create, sprite.Shader);
        }

    }
}
