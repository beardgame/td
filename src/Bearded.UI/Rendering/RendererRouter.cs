using System;
using System.Collections.Generic;
using System.Linq;

namespace Bearded.UI.Rendering
{
    public class RendererRouter : IRendererRouter
    {
        private readonly IReadOnlyList<(Type type, object renderer)> renderers;

        public RendererRouter(IEnumerable<(Type type, object renderer)> renderers)
        {
            this.renderers = new List<(Type type, object renderer)>(renderers).AsReadOnly();
        }

        public void Render<T>(T control)
        {
            GetRenderer<T>().Render(control);
        }

        protected virtual IRenderer<T> GetRenderer<T>()
        {
            return (IRenderer<T>) renderers.FirstOrDefault(
                t => t.type.IsAssignableFrom(typeof(T))
            ).renderer;
        }
    }
}
