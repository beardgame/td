﻿using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenTK.Input;
using MouseButtonEventArgs = Bearded.UI.EventArgs.MouseButtonEventArgs;

namespace Bearded.UI.Controls
{
    public class Button : CompositeControl
    {
        public event VoidEventHandler Clicked;

        public override void MouseButtonHit(MouseButtonEventArgs eventArgs)
        {
            base.MouseButtonHit(eventArgs);
            if (eventArgs.MouseButton == MouseButton.Left)
            {
                Clicked?.Invoke();
            }
            eventArgs.Handled = true;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
