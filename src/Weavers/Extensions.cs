using System.Collections.Generic;

namespace Weavers
{
    static class Extensions
    {
        public static void EnqueueAll<T>(this Queue<T> queue, IEnumerable<T> elements)
        {
            foreach (var elmt in elements) queue.Enqueue(elmt);
        }
    }
}
