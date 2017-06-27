using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities.Input;

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

        public override void HandleInput(InputContext input)
        {
            components.ForEach(c => c.HandleInput(input));
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