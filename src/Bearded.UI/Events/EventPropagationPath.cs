using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;

namespace Bearded.UI.Events
{
    sealed class EventPropagationPath
    {
        public static readonly EventPropagationPath Empty = new EventPropagationPath(new List<Control>());

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
            if (IsEmpty) return;

            var i = 0;

            while (i < path.Count)
            {
                previewEvent(path[i], eventArgs);
                if (eventArgs.Handled) break;
                i++;
            }

            if (i >= path.Count)
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

        private EventPropagationPath getPathFromIndex(int i)
            => new EventPropagationPath(path.Skip(i).ToList().AsReadOnly());

        public static (EventPropagationPath removed, EventPropagationPath added) CalculateDeviation(
            EventPropagationPath oldPath,
            EventPropagationPath newPath)
        {
            if (oldPath.IsEmpty) return (Empty, newPath);
            if (newPath.IsEmpty) return (oldPath, Empty);

            var shortestPathLength = Math.Min(oldPath.path.Count, newPath.path.Count);

            var firstDeviationIndex = 0;
            while (firstDeviationIndex < shortestPathLength)
            {
                if (oldPath.path[firstDeviationIndex] != newPath.path[firstDeviationIndex]) break;
                firstDeviationIndex++;
            }

            return (oldPath.getPathFromIndex(firstDeviationIndex), newPath.getPathFromIndex(firstDeviationIndex));
        }
    }
}
