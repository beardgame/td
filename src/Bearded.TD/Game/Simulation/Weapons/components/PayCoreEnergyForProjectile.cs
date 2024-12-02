using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Weapons.PayCoreEnergyForProjectile;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("payCoreEnergyForProjectile")]
sealed class PayCoreEnergyForProjectile(IParameters parameters)
    : Component<IParameters>(parameters), IListener<ShotProjectile>
{
    private FactionResources resources;

    internal interface IParameters : IParametersTemplate<IParameters>
    {
        double PerDamagePotential { get; }
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Activate()
    {
        Owner.TryFindFactionIncludingAncestors(out var faction);
        faction!.TryGetBehaviorIncludingAncestors(out resources!);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(ShotProjectile e)
    {
        var fromDamage = e.Damage.Amount.NumericValue * Parameters.PerDamagePotential;

        resources.ConsumeResources(fromDamage.CoreEnergy());
    }
}
