using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities.Input;

namespace Bearded.TD.UI.Components
{
    abstract class UIComponent
    {
        protected Bounds Bounds { get; }

        protected UIComponent(Bounds bounds)
        {
            Bounds = bounds;
        }

        public virtual void Update(UpdateEventArgs args) { }
        public virtual void HandleInput(InputContext input) { }
        public abstract void Draw(GeometryManager geometries);
    }
}
