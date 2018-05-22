namespace Bearded.UI.Rendering
{
    public interface IRenderer<in T>
    {
        void Render(T control);
    }
}
