namespace Bearded.TD.Rendering
{
    public struct ViewportSize
    {
        public int Width { get; }
        public int Height { get; }
        public int ScaledWidth { get; }
        public int ScaledHeight { get; }
        public float AspectRatio { get; }

        public ViewportSize(int width, int height, float uiScale = 1f)
        {
            Width = width;
            Height = height;
            ScaledWidth = (int) (width / uiScale);
            ScaledHeight = (int) (height / uiScale);
            AspectRatio = (float) Width / Height;
        }
    }
}
