using System;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class ControlContainer : CompositeControl
    {
        private Control? currentControl;
        private Action? currentDisposer;

        public ControlContainer()
        {
            IsClickThrough = true;
            IsVisible = false;
        }

        public void SetControl(Control control, Action? disposer = null)
        {
            ClearControl();
            Add(control);
            currentControl = control;
            currentDisposer = disposer;

            IsVisible = true;
        }

        public void ClearControl()
        {
            currentDisposer?.Invoke();
            currentDisposer = null;
            if (currentControl != null)
            {
                Remove(currentControl);
                currentControl = null;
            }

            IsVisible = false;
        }
    }
}
