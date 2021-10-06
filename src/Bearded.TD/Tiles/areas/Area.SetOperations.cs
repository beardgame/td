using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bearded.TD.Tiles
{
    static partial class Area
    {
        public static IArea Intersect(IArea left, IArea right)
        {
            // TODO: consider if this always needs to be precalculated
            return new HashSetArea(left.ToImmutableHashSet().Intersect(right.ToImmutableHashSet()));
        }

        public static IArea Union(IArea left, IArea right)
        {
            // TODO: consider if this always needs to be precalculated
            return new HashSetArea(left.ToImmutableHashSet().Union(right.ToImmutableHashSet()));
        }

        public static IArea Union(IEnumerable<IArea> areas)
        {
            // TODO: consider if this always needs to be precalculated
            var setBuilder = ImmutableHashSet.CreateBuilder<Tile>();
            foreach (var area in areas)
            {
                setBuilder.UnionWith(area);
            }

            return new HashSetArea(setBuilder.ToImmutable());
        }

        public static IArea Except(IArea left, IArea right)
        {
            // TODO: consider if this always needs to be precalculated
            return new HashSetArea(left.ToImmutableHashSet().Except(right.ToImmutableHashSet()));
        }
    }
}
