using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("arcEmitter")]
sealed class ArcEmitter : Component<ArcEmitter.IParameters>, IListener<FireWeapon>, IArcSource
{
    private const float maxExtraDamage = 0.6f;

    private IWeaponRange range = null!;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Arc { get; }
    }

    public ArcEmitter(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IWeaponRange>(Owner, Events, r => range = r);
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(FireWeapon e)
    {
        var arcs = ArcTree.Strike(Owner.Game, Owner, this, range.GetTilesInRange());

        var damageableTargetCount = arcs.Count(a => a.Target.GetComponents<IHealthEventReceiver>().Any());
        var damageFactorBasedOnTargetCount =
            1 + maxExtraDamage * (1 - MathF.Pow(0.5f, Math.Max(0, damageableTargetCount - 1)));
        var totalDamage = e.Damage * damageFactorBasedOnTargetCount;
        var damagePerTarget = totalDamage / Math.Max(1, damageableTargetCount);

        foreach (var arc in arcs)
        {
            var arcObject = ArcFactory.CreateArc(Parameters.Arc, Owner, arc.Source, arc.Target, damagePerTarget);
            Owner.Game.Add(arcObject);
        }
    }

    public IEnumerable<IArcModifier> ContinuationModifiers => Enumerable.Empty<IArcModifier>();

    public ArcTree.Continuation GetInitialArcParametersWithoutModifiers()
    {
        return ArcTree.Continuation.Initial(Owner);
    }
}
