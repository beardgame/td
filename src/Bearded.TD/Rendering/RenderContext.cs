﻿using Bearded.TD.UI.Layers;

namespace Bearded.TD.Rendering
{
    class RenderContext
    {
        public SurfaceManager Surfaces { get; }
        public GeometryManager Geometries { get; }
        public FrameCompositor Compositor { get; }

        public RenderContext()
        {
            Surfaces = new SurfaceManager();
            Geometries = new GeometryManager(Surfaces);
            Compositor = new FrameCompositor(Surfaces);
        }
        
        public void OnResize(ViewportSize viewportSize)
        {
            Compositor.SetViewportSize(viewportSize);
        }
    }
}
