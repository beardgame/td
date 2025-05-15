using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Weapons.PayCoreEnergyForProjectile;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("payCoreEnergyForProjectile")]
sealed class PayCoreEnergyForProjectile(IParameters parameters)
    : Component<IParameters>(parameters), IListener<ShotProjectile>
{
    private FactionResources? resources;

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
        if (!Owner.TryFindFactionIncludingAncestors(out var faction))
        {
            Owner.Game.Meta.Logger.Warning?.Log("Could not find owner faction for projectile core energy payment.");
            return;
        }

        faction.TryGetBehaviorIncludingAncestors(out resources);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(ShotProjectile e)
    {
        var fromDamage = e.Damage.Amount.NumericValue * Parameters.PerDamagePotential;

        State.Satisfies(resources is not null);
        resources?.ConsumeResources(fromDamage.CoreEnergy());
    }
}
