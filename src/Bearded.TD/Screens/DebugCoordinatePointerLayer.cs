using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Screens
{
    class DebugCoordinatePointerLayer : UIScreenLayer
    {
        private const float crossRadius = 12;
        private const float lineWidth = 1;
        private const float fontSize = 16;

        private bool visible;
        private Vector2 mousePos;

        public DebugCoordinatePointerLayer(ScreenLayerCollection parent, GeometryManager geometries) : base(parent, geometries) { }

        protected override bool DoHandleInput(InputContext input)
        {
            if (input.State.Keyboard.GetKeyState(Key.F9).Hit)
            {
                visible = !visible;
            }
            mousePos = input.MousePosition;
            return false;
        }

        public override void Draw()
        {
            base.Draw();
            if (!visible) return;

            var lineGeo = Geometries.ConsoleBackground;
            lineGeo.Color = Color.White;
            lineGeo.LineWidth = lineWidth;

            var offsetX = crossRadius * Vector2.UnitX;
            lineGeo.DrawLine(mousePos - offsetX, mousePos + offsetX);
            var offsetY = crossRadius * Vector2.UnitY;
            lineGeo.DrawLine(mousePos - offsetY, mousePos + offsetY);

            var textGeo = Geometries.ConsoleFont;
            textGeo.Color = Color.White;
            textGeo.Height = fontSize;
            textGeo.SizeCoefficient = Vector2.One;
            textGeo.DrawString(mousePos + .5f * offsetX + .5f * offsetY, mousePos.ToString());
        }
    }
}
