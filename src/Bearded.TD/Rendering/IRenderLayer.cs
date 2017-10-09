using OpenTK;

namespace Bearded.TD.Rendering
{
    interface IRenderLayer {
        Matrix4 ViewMatrix { get; }
        Matrix4 ProjectionMatrix { get; }
        RenderOptions RenderOptions { get; }
        void Draw();
    }
}
