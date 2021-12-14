using System;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MouseButtonEventArgs = Bearded.UI.EventArgs.MouseButtonEventArgs;
using MouseEventArgs = Bearded.UI.EventArgs.MouseEventArgs;

namespace Bearded.TD.UI.Controls;

sealed class ButtonBackgroundEffect : Control
{
    private readonly Func<bool> isButtonEnabled;

    public bool ButtonIsEnabled => isButtonEnabled();
    public bool MouseIsOver { get; private set; }
    public bool MouseIsDown { get; private set; }

    public ButtonBackgroundEffect(Func<bool> isButtonEnabled)
    {
        this.isButtonEnabled = isButtonEnabled;
    }

    protected override void OnAddingToParent()
    {
        if (Parent is Control control)
        {
            control.MouseMove += onParentMouseMove;
            control.MouseExit += onParentMouseExit;
            control.MouseButtonDown += onParentMouseButtonDown;
            control.MouseButtonRelease += onParentMouseButtonRelease;
        }
    }

    protected override void OnRemovingFromParent()
    {
        if (Parent is Control control)
        {
            control.MouseMove -= onParentMouseMove;
            control.MouseExit -= onParentMouseExit;
            control.MouseButtonDown -= onParentMouseButtonDown;
            control.MouseButtonRelease -= onParentMouseButtonRelease;
        }

        MouseIsOver = false;
        MouseIsDown = false;
    }

    private void onParentMouseMove(MouseEventArgs args)
    {
        MouseIsOver = true;
    }

    private void onParentMouseExit(MouseEventArgs args)
    {
        MouseIsOver = false;
    }

    private void onParentMouseButtonDown(MouseButtonEventArgs t)
    {
        if (t.MouseButton == MouseButton.Left)
        {
            MouseIsDown = true;
        }
    }

    private void onParentMouseButtonRelease(MouseButtonEventArgs t)
    {
        if (t.MouseButton == MouseButton.Left)
        {
            MouseIsDown = false;
        }
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}