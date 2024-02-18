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
        MouseIsOver = true;
        StateChanged?.Invoke();
    }

    private void onMouseExit(MouseEventArgs args)
    {
        MouseIsOver = false;
        StateChanged?.Invoke();
    }

    private void onMouseButtonDown(MouseButtonEventArgs t)
    {
        if (t.MouseButton == MouseButton.Left)
        {
            MouseIsDown = true;
            StateChanged?.Invoke();
        }
    }

    private void onMouseButtonRelease(MouseButtonEventArgs t)
    {
        if (t.MouseButton == MouseButton.Left)
        {
            MouseIsDown = false;
            StateChanged?.Invoke();
        }
    }
}
