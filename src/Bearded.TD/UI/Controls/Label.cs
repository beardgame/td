using amulware.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK;

namespace Bearded.TD.UI.Controls
{
    class Label : Control
    {
        public string Text { get; set; } = "a label";
        public double FontSize { get; set; } = 24;
        public Vector2d TextAnchor { get; set; } = new Vector2d(0.5, 0.5);
        public Color Color { get; set; } = Color.White;

        public Label()
        {
        }

        public Label(string text)
        {
            Text = text;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
