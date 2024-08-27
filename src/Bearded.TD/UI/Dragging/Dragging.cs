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
        this TControl control, DragScope scope, DragAction<TControl> drag, Action? dragEnd = null)
        where TControl : Control
    {
        return AddDragging(control, scope, move => drag(control, move), dragEnd);
    }

    public static IDragController AddDragging<TControl>(
        this TControl control, DragScope scope, DragAction drag, Action? dragEnd = null)
        where TControl : Control
    {
        var controller = new Controller(control, scope, drag, dragEnd);
        controller.Enable();
        return controller;
    }

    private sealed class Controller(Control source, DragScope scope, DragAction drag, Action? dragEnd)
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
            if (Parent != null)
            {
                // This can happen in rare cases, though I'm unsure why exactly - the mouse-up must not be detected.
                RemoveFromParent();
            }

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
            var delta = eventArgs.MousePosition - lastDragMousePosition;

            drag(new DragEvent(eventArgs.MousePosition, delta));

            lastDragMousePosition = eventArgs.MousePosition;
        }

        private void endDrag()
        {
            if (Parent != null)
            {
                dragEnd?.Invoke();
                RemoveFromParent();
            }
        }

        protected override void RenderStronglyTyped(IRendererRouter r) { }
    }
}
