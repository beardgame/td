using System;
using System.Collections.Generic;

namespace Bearded.TD.UI.Components
{
    class Menu : CompositeComponent
    {
        private const float spaceBetweenButtons = 5;
        private const float buttonHeight = 50;

        private readonly FocusContainer focusContainer = new FocusContainer();

        public Menu(Bounds bounds, IEnumerable<Func<Bounds, FocusableUIComponent>> itemFactories) : base(bounds)
        {
            var y = bounds.YStart;

            foreach (var factory in itemFactories)
            {
                addMenuItem(factory(new Bounds(Bounds.X, new FixedSizeAndOffsetDimension(Bounds.Y, buttonHeight, y, 0))));
                y += buttonHeight + spaceBetweenButtons;
            }
        }

        private void addMenuItem(FocusableUIComponent component)
        {
            AddComponent(component);
            focusContainer.RegisterFocusable(component);
        }
    }
}
