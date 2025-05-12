using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Geometry;
using static Bearded.TD.Constants.Content.CoreUI;
using Generic = System.Collections.Generic;

namespace Bearded.TD.Game.Simulation.Weapons;

static class TargetingMode
{
    // ReSharper disable UnusedMember.Global, MemberCanBePrivate.Global
    public static ITargetingMode Default => LeastRotation;
    public static readonly ITargetingMode Random = new RandomTargetingMode();
    public static readonly ITargetingMode Arbitrary = new ArbitraryTargetingMode();

    public static readonly ITargetingMode LeastRotation = new LeastRotationTargetingMode();
    public static readonly ITargetingMode HighestHealth = new HighestHealthTargetingMode();
    public static readonly ITargetingMode LowestHealth = new LowestHealthTargetingMode();
    public static readonly ITargetingMode ClosestToBase = new ClosestToBaseTargetingMode();
    // ReSharper restore UnusedMember.Global, MemberCanBePrivate.Global

    public static readonly ImmutableArray<ITargetingMode> AllPlayerSelectable =
        [LeastRotation, HighestHealth, LowestHealth, ClosestToBase];

    private sealed class RandomTargetingMode : ITargetingMode
    {
        private static readonly Random random = new();

        public string Name => "Random";
        public ModAwareSpriteId Icon => Sprites.Targeting("perspective-dice-six-faces-random");

        public GameObject? SelectTarget(IEnumerable<GameObject> candidates, TargetingContext context) =>
            candidates.RandomElementOrDefault(random);
    }

    private sealed class ArbitraryTargetingMode : ITargetingMode
    {
        public string Name => "Arbitrary";
        public ModAwareSpriteId Icon => Sprites.Targeting("perspective-dice-six-faces-random");

        public GameObject? SelectTarget(IEnumerable<GameObject> candidates, TargetingContext context) =>
            candidates.FirstOrDefault();
    }

    private sealed class LeastRotationTargetingMode : ITargetingMode
    {
        public string Name => "Least rotation";
        public ModAwareSpriteId Icon => Sprites.Targeting("striking-arrows");

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

    private sealed class HighestHealthTargetingMode : ITargetingMode
    {
        public string Name => "Highest health";
        public ModAwareSpriteId Icon => Sprites.Targeting("heart-plus");

        public GameObject? SelectTarget(IEnumerable<GameObject> candidates, TargetingContext context)
        {
            return candidates.MaxBy(c => c.CurrentTotalHitPointsOrNull()?.NumericValue, nullsFirst<float?>());
        }
    }

    private sealed class LowestHealthTargetingMode : ITargetingMode
    {
        public string Name => "Lowest health";
        public ModAwareSpriteId Icon => Sprites.Targeting("heart-minus");

        public GameObject? SelectTarget(IEnumerable<GameObject> candidates, TargetingContext context)
        {
            return candidates.MinBy(c => c.CurrentTotalHitPointsOrNull()?.NumericValue, nullsLast<float?>());
        }
    }

    private sealed class ClosestToBaseTargetingMode : ITargetingMode
    {
        public string Name => "Closest to base";
        public ModAwareSpriteId Icon => Sprites.Targeting("path-distance");

        public GameObject? SelectTarget(IEnumerable<GameObject> candidates, TargetingContext context)
        {
            return candidates.MinBy(c => distanceToBase(context, c));
        }

        private static int distanceToBase(TargetingContext context, GameObject c)
        {
            return context.Navigator.GetDistanceToClosestSink(Level.GetTile(c.Position));
        }
    }

    private static Generic.Comparer<T> nullsFirst<T>(IComparer<T>? original = null)
    {
        original ??= Generic.Comparer<T>.Default;
        return Generic.Comparer<T>.Create((obj1, obj2) => (obj1, obj2) switch
        {
            (null, null) => 0,
            (null, _) => 1,
            (_, null) => -1,
            _ => original.Compare(obj1, obj2),
        });
    }

    private static Generic.Comparer<T> nullsLast<T>(IComparer<T>? original = null)
    {
        original ??= Generic.Comparer<T>.Default;
        return Generic.Comparer<T>.Create((obj1, obj2) => (obj1, obj2) switch
        {
            (null, null) => 0,
            (null, _) => -1,
            (_, null) => 1,
            _ => original.Compare(obj1, obj2),
        });
    }
}
