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
                var child = parent.Children.Reverse().FirstOrDefault(childSelector.Invoke);
                if (child != null)
                {
                    path.Add(child);
                }

                parent = child as IControlParent;
            }

            return new EventPropagationPath(path.AsReadOnly());
        }

        public static EventPropagationPath FindPropagationPath(IControlParent root, Control leaf)
        {
            var parent = leaf.Parent;
            var path = new List<Control>{ leaf };

            while (parent != root)
            {
                if (!(parent is Control parentAsControl))
                {
                    // Tried creating path between unconnected controls.
                    return EventPropagationPath.Empty;
                }
                path.Add(parentAsControl);
                
                parent = parentAsControl.Parent;
            }

            path.Reverse();
            return new EventPropagationPath(path.AsReadOnly());
        }
    }
}
