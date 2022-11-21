using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Weapons;

static class TargetingMode
{
    public static readonly ITargetingMode Arbitrary = new ArbitraryTargetingMode();

    private sealed class ArbitraryTargetingMode : ITargetingMode
    {
        public string Name => "Arbitrary";

        public GameObject? SelectTarget(IEnumerable<GameObject> candidates, TargetingContext context) =>
            candidates.FirstOrDefault();
    }

    [UsedImplicitly]
    public static readonly ITargetingMode LeastRotation = new LeastRotationTargetingMode();

    private sealed class LeastRotationTargetingMode : ITargetingMode
    {
        public string Name => "Least rotation";

        public GameObject? SelectTarget(IEnumerable<GameObject> candidates, TargetingContext context)
        {
            if (context.CurrentAimDirection == null)
            {
                return Arbitrary.SelectTarget(candidates, context);
            }

            return candidates.MinBy(c => angleError(c, context).MagnitudeInRadians);
        }

        private static Angle angleError(GameObject candidate, TargetingContext context)
        {
            var directionToCandidate = (candidate.Position - context.WeaponPosition).XY().Direction;
            return Angle.Between(directionToCandidate, context.CurrentAimDirection.GetValueOrDefault());
        }
    }

    [UsedImplicitly]
    public static readonly ITargetingMode HighestHealth = new HighestHealthTargetingMode();

    private sealed class HighestHealthTargetingMode : ITargetingMode
    {
        public string Name => "Highest health";

        public GameObject? SelectTarget(IEnumerable<GameObject> candidates, TargetingContext context)
        {
            return candidates.MaxBy(
                c => c.GetComponents<IHealth>().FirstOrDefault()?.CurrentHealth.NumericValue, nullsFirst<int?>());
        }
    }

    [UsedImplicitly]
    public static readonly ITargetingMode LowestHealth = new LowestHealthTargetingMode();

    private sealed class LowestHealthTargetingMode : ITargetingMode
    {
        public string Name => "Lowest health";

        public GameObject? SelectTarget(IEnumerable<GameObject> candidates, TargetingContext context)
        {
            return candidates.MinBy(
                c => c.GetComponents<IHealth>().FirstOrDefault()?.CurrentHealth.NumericValue, nullsLast<int?>());
        }
    }

    [UsedImplicitly]
    public static readonly ITargetingMode ClosestToBase = new ClosestToBaseTargetingMode();

    private sealed class ClosestToBaseTargetingMode : ITargetingMode
    {
        public string Name => "Closest to base";

        public GameObject? SelectTarget(IEnumerable<GameObject> candidates, TargetingContext context)
        {
            return candidates.MinBy(c => distanceToBase(context, c));
        }

        private static int distanceToBase(TargetingContext context, GameObject c)
        {
            return context.Navigator.GetDistanceToClosestSink(Level.GetTile(c.Position));
        }
    }

    public static ITargetingMode Default => LeastRotation;

    public static readonly ImmutableArray<ITargetingMode> All =
        ImmutableArray.Create(LeastRotation, HighestHealth, LowestHealth, ClosestToBase);

    private static IComparer<T> nullsFirst<T>(IComparer<T>? original = null)
    {
        original ??= System.Collections.Generic.Comparer<T>.Default;
        return System.Collections.Generic.Comparer<T>.Create((obj1, obj2) => (obj1, obj2) switch
        {
            (null, null) => 0,
            (null, _) => 1,
            (_, null) => -1,
            _ => original.Compare(obj1, obj2)
        });
    }

    private static IComparer<T> nullsLast<T>(IComparer<T>? original = null)
    {
        original ??= System.Collections.Generic.Comparer<T>.Default;
        return System.Collections.Generic.Comparer<T>.Create((obj1, obj2) => (obj1, obj2) switch
        {
            (null, null) => 0,
            (null, _) => -1,
            (_, null) => 1,
            _ => original.Compare(obj1, obj2)
        });
    }
}
