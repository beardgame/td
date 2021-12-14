using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.UI;

sealed class BackgroundBoxRenderer : IRenderer<BackgroundBox>
{
    private readonly IShapeDrawer2<Color> drawer;

    public BackgroundBoxRenderer(IShapeDrawer2<Color> drawer)
    {
        this.drawer = drawer;
    }

    public void Render(BackgroundBox control)
    {
        var frame = control.Frame;

        drawer.FillRectangle((Vector2)frame.TopLeft, (Vector2)frame.Size, control.Color);
    }
}