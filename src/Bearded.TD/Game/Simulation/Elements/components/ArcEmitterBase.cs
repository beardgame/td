using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Elements;

abstract class ArcEmitterBase<T> : Component<T>, IArcSource
    where T : IArcEmissionParameters, IParametersTemplate<T>
{
    private const float maxExtraDamage = 0.6f;

    protected ArcEmitterBase(T parameters) : base(parameters) { }

    protected void EmitArc(UntypedDamage baseDamage, ImmutableArray<Tile> tilesInRange)
    {
        var arcs = ArcTree.Strike(Owner.Game, Owner, this, tilesInRange);

        var damageableTargetCount = arcs.Count(a => a.Target.GetComponents<IHealthEventReceiver>().Any());
        var damageFactorBasedOnTargetCount =
            1 + maxExtraDamage * (1 - MathF.Pow(0.5f, Math.Max(0, damageableTargetCount - 1)));
        var totalDamage = baseDamage * damageFactorBasedOnTargetCount;
        var damagePerTarget = totalDamage / Math.Max(1, damageableTargetCount);

        foreach (var arc in arcs)
        {
            var arcObject = ArcFactory.CreateArc(Parameters.Arc, Owner, arc.Source, arc.Target, damagePerTarget);
            Owner.Game.Add(arcObject);
        }
    }

    public IEnumerable<IArcModifier> ContinuationModifiers => Enumerable.Empty<IArcModifier>();

    public ArcTree.Continuation GetInitialArcParametersWithoutModifiers() => ArcTree.Continuation.Initial(
        Owner,
        branches: Parameters.Branches,
        bounces: Parameters.Bounces,
        maxBounceDistance: Parameters.MaxBounceDistance);
}
