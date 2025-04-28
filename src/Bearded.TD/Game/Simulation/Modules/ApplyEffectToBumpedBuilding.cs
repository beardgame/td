using System.Collections.Immutable;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

using static ApplyEffectToBumpedBuilding;

[Component("applyEffectToBumpedBuilding")]
sealed class ApplyEffectToBumpedBuilding(IParameters parameters)
    : Component<IParameters>(parameters), IListener<BumpedBuilding>, IEffectApplier
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        TimeSpan EffectDuration { get; }

        ImmutableArray<IUpgradeEffect> Effects { get; }
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public void HandleEvent(BumpedBuilding e)
    {
        Owner.Sync(ApplyEffects.Command, Owner, e.Building);
    }

    public void ApplyTo(GameObject target)
    {
        var upgrade = Upgrade.FromEffects(Parameters.Effects);

        if (!target.CanApplyUpgrade(upgrade))
            return;

        var receipt = target.ApplyUpgrade(upgrade);

        target.Delay(receipt.Rollback, Parameters.EffectDuration, DelayMode.OnTimeOutOrDeleting);
    }
}
