using System.Collections.Generic;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;

namespace Bearded.UI.Events
{
    sealed class EventPropagationPath
    {
        private readonly IList<Control> path;

        public bool IsEmpty => path.Count == 0;

        public EventPropagationPath(IList<Control> path)
        {
            this.path = path;
        }

        public void PropagateEvent<T>(
            T eventArgs,
            EventRouter.RoutedEvent<T> previewEvent,
            EventRouter.RoutedEvent<T> bubbleEvent) where T : RoutedEventArgs
        {
            var i = 0;

            while (i < path.Count)
            {
                previewEvent(path[i], eventArgs);
                if (eventArgs.Handled) break;
                i++;
            }

            if (i > path.Count)
            {
                i = path.Count - 1;
            }

            while (i >= 0)
            {
                bubbleEvent(path[i], eventArgs);
                if (eventArgs.Handled) break;
                i--;
            }
        }
    }
}
