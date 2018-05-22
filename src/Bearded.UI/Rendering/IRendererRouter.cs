namespace Bearded.UI.Rendering
{
    public interface IRendererRouter
    {
        void Render<T>(T control);
    }
}
