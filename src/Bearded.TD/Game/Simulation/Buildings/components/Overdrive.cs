using System;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Elements.events;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class Overdrive : Component, IPreviewListener<PreviewTemperatureTick>
{
    private static readonly TemperatureRate baseTemperatureIncreaseRate = new(14);
    private const float temperatureIncreaseExponent = 1.04f;

    private IUpgradeReceipt? upgradeReceipt;
    private Instant? startTime;

    protected override void OnAdded() {}

    public override void Activate()
    {
        base.Activate();

        var ids = Owner.Game.GamePlayIds;

        var fireRate = new ModifyParameterReversibly(
            AttributeType.FireRate, damageModification(ids), UpgradePrerequisites.Empty);
        var damageOverTime = new ModifyParameterReversibly(
            AttributeType.DamageOverTime, damageModification(ids), UpgradePrerequisites.Empty);
        var upgrade = Upgrade.FromEffects(fireRate, damageOverTime);

        upgradeReceipt = Owner.ApplyUpgrade(upgrade);
        startTime = Owner.Game.Time;

        Events.Subscribe(this);
    }

    private static ModificationWithId damageModification(IdManager ids)
        => new(ids.GetNext<Modification>(), Modification.MultiplyWith(1.5));

    public override void OnRemoved()
    {
        upgradeReceipt?.Rollback();
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void PreviewEvent(ref PreviewTemperatureTick @event)
    {
        if (@event.Now - startTime is not { } timeSinceStart || timeSinceStart <= TimeSpan.Zero)
        {
            return;
        }

        var factor = MathF.Pow(temperatureIncreaseExponent, (float) timeSinceStart.NumericValue);
        var rate = baseTemperatureIncreaseRate * factor;
        @event = @event.WithAddedRate(rate);
    }
}
