using System;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Dragging;

static class Dragging
{
    public static IDragController AddDragging<TControl>(
        this TControl control, DragScope scope, DragAction<TControl> drag)
        where TControl : Control
    {
        return AddDragging(control, scope, move => drag(control, move));
    }

    public static IDragController AddDragging<TControl>(
        this TControl control, DragScope scope, DragAction drag)
        where TControl : Control
    {
        var controller = new Controller(control, scope, drag);
        controller.Enable();
        return controller;
    }

    private sealed class Controller(Control source, DragScope scope, DragAction drag)
        : Control, IDragController
    {
        private Vector2d lastDragMousePosition;

        public void Enable()
        {
            source.MouseButtonDown += onMouseButtonDown;
        }

        public void Disable()
        {
            source.MouseButtonDown -= onMouseButtonDown;

            endDrag();
        }

        private void onMouseButtonDown(MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.MouseButton != MouseButton.Left)
            {
                return;
            }

            startDrag(eventArgs);
            eventArgs.Handled = true;
        }

        public override void PreviewMouseMoved(MouseEventArgs eventArgs)
        {
            if (!eventArgs.MouseButtons.Left)
            {
                endDrag();
                return;
            }

            continueDrag(eventArgs);
            eventArgs.Handled = true;
        }

        private void startDrag(MouseButtonEventArgs eventArgs)
        {
            var parent = scope switch
            {
                DragScope.WithinParent => source.Parent,
                DragScope.Anywhere => source.FindRoot(),
                _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null),
            } ?? throw new InvalidOperationException("Tried starting dragging control with no parent or root.");

            parent.Add(this);

            lastDragMousePosition = eventArgs.MousePosition;
        }

        private void continueDrag(MouseEventArgs eventArgs)
        {
            var move = eventArgs.MousePosition - lastDragMousePosition;

            drag(move);

            lastDragMousePosition = eventArgs.MousePosition;
        }

        private void endDrag()
        {
            if (Parent != null)
            {
                RemoveFromParent();
            }
        }

        protected override void RenderStronglyTyped(IRendererRouter r) { }
    }
}
