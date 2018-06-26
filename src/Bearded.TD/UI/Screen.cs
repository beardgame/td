using Bearded.TD.Utilities;

namespace Bearded.TD.UI
{
    class Screen : Bounds
    {
        private readonly ScreenDimension screenX;
        private readonly ScreenDimension screenY;

        public static Screen GetCanvas() => new Screen(new ScreenWidth(), new ScreenHeight());

        private Screen(ScreenDimension x, ScreenDimension y) : base(x, y)
        {
            screenX = x;
            screenY = y;
        }

        public void OnResize(ViewportSize viewportSize)
        {
            screenX.OnResize(viewportSize);
            screenY.OnResize(viewportSize);
        }

        private abstract class ScreenDimension : IDimension
        {
            public float Min => 0;
            public float Max { get; private set; }

            public void OnResize(ViewportSize viewportSize)
            {
                Max = GetDimension(viewportSize);
            }

            protected abstract float GetDimension(ViewportSize size);
        }

        private class ScreenWidth : ScreenDimension
        {
            protected override float GetDimension(ViewportSize size) => size.ScaledWidth;
        }

        private class ScreenHeight : ScreenDimension
        {
            protected override float GetDimension(ViewportSize size) => size.ScaledHeight;
        }
    }
}