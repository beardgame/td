using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("disableDragAfterSprint")]
sealed class DisableDragAfterSprint : Component<DisableDragAfterSprint.IParameters>, IListener<StoppedSprinting>
{
    private IUpgradeReceipt? upgradeReceipt;
    private Instant resetTime;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        TimeSpan Duration { get; }
    }

    public DisableDragAfterSprint(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (upgradeReceipt != null && Owner.Game.Time > resetTime)
            reset();
    }

    public void HandleEvent(StoppedSprinting @event)
    {
        reset();

        var mod = new ModificationWithId(Owner.Game.GamePlayIds.GetNext<Modification>(), Modification.MultiplyWith(0));
        var noDrag = new ModifyParameterReversibly(AttributeType.Drag, mod, UpgradePrerequisites.Empty);
        var upgrade = Upgrade.FromEffects(noDrag);

        upgradeReceipt = Owner.ApplyUpgrade(upgrade);

        resetTime = Owner.Game.Time + Parameters.Duration;
    }

    private void reset()
    {
        upgradeReceipt?.Rollback();
        upgradeReceipt = null;
    }
}

