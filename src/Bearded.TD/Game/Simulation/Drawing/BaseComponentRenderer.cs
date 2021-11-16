using Bearded.Graphics.Vertices;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Drawing
{
    abstract class BaseComponentRenderer<T> : Component<T>, IComponentRenderer
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
            drawRecursively(Owner);
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

        public abstract void DrawSprite<TVertex, TVertexData>(
            SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 position, float size, float angle, TVertexData data)
            where TVertex : struct, IVertexData;
    }
}
