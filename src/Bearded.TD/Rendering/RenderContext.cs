
namespace Bearded.TD.Rendering
{
    class RenderContext
    {
        public SurfaceManager Surfaces { get; }
        public SpriteManager Sprites { get; }
        public FrameCompositor Compositor { get; }

        public RenderContext()
        {
            Surfaces = new SurfaceManager();
            Sprites = new SpriteManager(Surfaces);
            Compositor = new FrameCompositor(Surfaces);
        }
    }
}