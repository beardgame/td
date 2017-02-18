namespace Bearded.TD.Rendering
{
    public struct ViewportSize
    {
        public int Width { get; }
        public int Height { get; }
        public float AspectRatio { get; }

        public ViewportSize(int width, int height)
        {
            Width = width;
            Height = height;
            AspectRatio = (float) Width / Height;
        }
    }
}
