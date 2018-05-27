using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;

namespace Bearded.UI.Events
{
    static class EventRouter
    {
        public delegate void RoutedEvent<in T>(Control control, T eventData) where T : RoutedEventArgs;

        public static EventPropagationPath FindPropagationPath(IControlParent root, Predicate<Control> childSelector)
        {
            var parent = root;
            var path = new List<Control>();

            while (parent != null)
            {
                var child = parent.Children.FirstOrDefault(childSelector.Invoke);
                if (child != null)
                {
                    path.Add(child);
                }

                parent = child as IControlParent;
            }

            return new EventPropagationPath(path.AsReadOnly());
        }
    }
}
