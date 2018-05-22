using System;
using System.Collections.Generic;

namespace Bearded.UI.Rendering
{
    public class CachedRendererRouter : RendererRouter
    {
        private readonly Dictionary<Type, object> rendererCache = new Dictionary<Type, object>();
        
        public CachedRendererRouter(IEnumerable<(Type type, object renderer)> renderers) : base(renderers) { }

        protected override IRenderer<T> GetRenderer<T>()
        {
            if (rendererCache.TryGetValue(typeof(T), out var r))
            {
                return (IRenderer<T>) r;
            }

            var renderer = base.GetRenderer<T>();
            rendererCache.Add(typeof(T), renderer);
            return renderer;
        }
    }
}
