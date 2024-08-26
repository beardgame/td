using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.Utilities;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Factories;

sealed class MouseStateObserver
{
    public bool MouseIsOver { get; private set; }
    public bool MouseIsDown { get; private set; }

    public event VoidEventHandler? StateChanged;

    public MouseStateObserver(Control target)
    {
        target.MouseMove += onMouseMove;
        target.MouseExit += onMouseExit;
        target.MouseButtonDown += onMouseButtonDown;
        target.MouseButtonRelease += onMouseButtonRelease;
    }

    private void onMouseMove(MouseEventArgs args)
    {
        set(down: args.MouseButtons.Left, over: true);
    }

    private void onMouseExit(MouseEventArgs args)
    {
        set(over: false);
    }

    private void onMouseButtonDown(MouseButtonEventArgs t)
    {
        if (t.MouseButton == MouseButton.Left)
        {
            set(down: true);
        }
    }

    private void onMouseButtonRelease(MouseButtonEventArgs t)
    {
        if (t.MouseButton == MouseButton.Left)
        {
            set(down: false);
        }
    }

    private void set(bool? down = null, bool? over = null)
    {
        var newState = (down ?? MouseIsDown, over ?? MouseIsOver);

        if (newState == (MouseIsDown, MouseIsOver))
        {
            return;
        }

        (MouseIsDown, MouseIsOver) = newState;
        StateChanged?.Invoke();
    }
}
