using System.Collections.Immutable;
using System.Linq;
using Bearded.Utilities;

namespace Bearded.TD.Utilities
{
    static class IdManagerExtensions
    {
        public static ImmutableArray<Id<T>> GetBatch<T>(this IdManager idManager, int batchSize)
        {
            return Enumerable.Range(0, batchSize).Select(_ => idManager.GetNext<T>()).ToImmutableArray();
        }
    }
}
