using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;

namespace Bearded.TD.UI.Components
{
    abstract class CompositeComponent : UIComponent
    {
        private readonly List<UIComponent> components = new List<UIComponent>();

        protected CompositeComponent(Bounds bounds) : base(bounds)
        {
        }

        public override void Update(UpdateEventArgs args)
        {
            components.ForEach(c => c.Update(args));
        }

        public override void HandleInput(InputState inputState)
        {
            components.ForEach(c => c.HandleInput(inputState));
        }

        public override void Draw(GeometryManager geometries)
        {
            components.ForEach(c => c.Draw(geometries));
        }

        protected void AddComponent(UIComponent component)
        {
            components.Add(component);
        }
    }
}