using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;

namespace Bearded.UI.Events
{
    static class EventRouter
    {
        internal delegate void RoutedEvent<in T>(Control control, T eventData) where T : RoutedEventArgs;

        internal static IList<Control> FindPropagationPath(IControlParent root, Predicate<Control> childSelector)
        {
            var parent = root;
            var path = new List<Control>();

            while (parent != null)
            {
                var child = root.Children.FirstOrDefault(childSelector.Invoke);
                if (child != null)
                {
                    path.Add(child);
                }

                parent = child as IControlParent;
            }

            return path.AsReadOnly();
        }

        internal static void PropagateEvent<T>(
            IList<Control> propagationPath,
            T eventArgs,
            RoutedEvent<T> previewEvent,
            RoutedEvent<T> bubbleEvent) where T : RoutedEventArgs
        {
            var i = 0;

            while (i < propagationPath.Count)
            {
                previewEvent(propagationPath[i], eventArgs);
                if (eventArgs.Handled) break;
                i++;
            }

            if (i > propagationPath.Count)
            {
                i = propagationPath.Count - 1;
            }

            while (i >= 0)
            {
                bubbleEvent(propagationPath[i], eventArgs);
                if (eventArgs.Handled) break;
                i--;
            }
        }
    }
}
