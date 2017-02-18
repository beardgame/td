using amulware.Graphics;
using OpenTK;

namespace Bearded.TD.Rendering
{
    class ConsoleScreenLayer : UIScreenLayer
    {
        public ConsoleScreenLayer(GeometryManager geometries) : base(geometries)
        { }

        public override void Draw()
        {
            Geometries.ConsoleBackground.Color = Color.Black.WithAlpha(.7f).Premultiplied;
            Geometries.ConsoleBackground.DrawRectangle(-640, 0, 1280, 320);

            Geometries.ConsoleFont.Color = Color.White.Premultiplied;
            Geometries.ConsoleFont.Height = 14;
            Geometries.ConsoleFont.DrawString(new Vector2(-640, 0), "Hello, world!");
        }
    }
}
