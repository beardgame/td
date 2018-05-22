namespace Bearded.UI.Rendering
{
    sealed class NoRenderer<T> : IRenderer<T>
    {
        public void Render(T control) {}
    }
}
