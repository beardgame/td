using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
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

            return candidates.Select(c => (Candidate: c, AngleError: angleError(c, context)))
                .OrderBy(tuple => tuple.AngleError.MagnitudeInRadians)
                .Select(tuple => tuple.Candidate)
                .FirstOrDefault();
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
            return candidates.Select(c => (Candidate: c, Health: c.GetComponents<IHealth>().FirstOrDefault()))
                .OrderBy(tuple => tuple.Health is null ? 1 : 0)
                .ThenByDescending(tuple => tuple.Health?.CurrentHealth ?? HitPoints.Zero)
                .Select(tuple => tuple.Candidate)
                .FirstOrDefault();
        }
    }

    [UsedImplicitly]
    public static readonly ITargetingMode LowestHealth = new LowestHealthTargetingMode();

    private sealed class LowestHealthTargetingMode : ITargetingMode
    {
        public string Name => "Lowest health";

        public GameObject? SelectTarget(IEnumerable<GameObject> candidates, TargetingContext context)
        {
            return candidates.Select(c => (Candidate: c, Health: c.GetComponents<IHealth>().FirstOrDefault()))
                .OrderBy(tuple => tuple.Health is null ? 1 : 0)
                .ThenBy(tuple => tuple.Health?.CurrentHealth ?? HitPoints.Zero)
                .Select(tuple => tuple.Candidate)
                .FirstOrDefault();
        }
    }

    public static ITargetingMode Default => LeastRotation;

    public static readonly ImmutableArray<ITargetingMode> All =
        ImmutableArray.Create(LeastRotation, HighestHealth, LowestHealth);
}
