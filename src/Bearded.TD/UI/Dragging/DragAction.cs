using Bearded.UI.Controls;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Dragging;

delegate void DragAction(Vector2d move);

delegate void DragAction<in TControl>(TControl control, Vector2d move)
    where TControl : Control;

static class DragActions
{
    public static DragAction<Control> DefaultMove { get; } = (control, move) =>
    {
        var frame = control.Frame;

        control.Anchor(a => a
            .Left(margin: frame.TopLeft.X + move.X, width: frame.Size.X)
            .Top(margin: frame.TopLeft.Y + move.Y, height: frame.Size.Y)
        );
    };
}
