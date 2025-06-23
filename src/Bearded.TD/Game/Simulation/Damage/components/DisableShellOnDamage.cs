using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;
using static Bearded.TD.Game.Simulation.Damage.DisableShellOnDamage;

namespace Bearded.TD.Game.Simulation.Damage;

[Component("disableShellOnDamage")]
sealed class DisableShellOnDamage(IParameters parameters) : Component<IParameters>(parameters), IListener<TookDamage>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        double Probability { get; }

        TimeSpan Duration { get; }

        DamageType DamageType { get; }

        DamageShell Shell { get; }
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    protected override void OnRemovedInternal()
    {
        Events.Unsubscribe(this);
    }

    public void HandleEvent(TookDamage @event)
    {
        if (@event.Pool.Shell != Parameters.Shell) return;
        if (@event.Result.ExactDamageDone.Type != Parameters.DamageType) return;

        if (!Random.Shared.NextBool(Parameters.Probability)) return;

        var receipt = @event.Pool.Disable();
        Owner.Delay(receipt.Undo, Parameters.Duration);
    }
}
