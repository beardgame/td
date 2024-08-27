using Bearded.UI.Controls;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Dragging;

readonly record struct DragEvent(Vector2d MousePosition, Vector2d MouseDelta);

delegate void DragAction(DragEvent move);

delegate void DragAction<in TControl>(TControl control, DragEvent move)
    where TControl : Control;

static class DragActions
{
    public static DragAction<Control> DefaultMove { get; } = (control, move) =>
    {
        var frame = control.Frame;

        control.Anchor(a => a
            .Left(margin: frame.TopLeft.X + move.MouseDelta.X, width: frame.Size.X)
            .Top(margin: frame.TopLeft.Y + move.MouseDelta.Y, height: frame.Size.Y)
        );
    };
}
