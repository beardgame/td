namespace Bearded.TD.Rendering
{
    public struct ViewportSize
    {
        public int Width { get; }
        public int Height { get; }
        public float AspectRatio { get; }

        public ViewportSize(int width, int height, float uiScale = 1f)
        {
            Width = (int) (width / uiScale);
            Height = (int) (height / uiScale);
            AspectRatio = (float) Width / Height;
        }
    }
}
