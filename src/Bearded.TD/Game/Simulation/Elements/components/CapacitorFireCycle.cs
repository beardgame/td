using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("capacitorFireCycle")]
sealed class CapacitorFireCycle : WeaponCycleHandler<CapacitorFireCycle.IParameters>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        float ChargeToDamageRate { get; }
    }

    private ICapacitor? capacitor;

    public CapacitorFireCycle(IParameters parameters)
        : base(parameters)
    {
    }

    public override void Activate()
    {
        base.Activate();
        Owner.TryGetSingleComponentInOwnerTree(out capacitor);
    }

    protected override void UpdateShooting()
    {
        if (capacitor is null) return;
        if (capacitor.CurrentCharge < capacitor.MaxCharge) return;

        var charge = capacitor.Discharge();
        var damage = toDamage(charge);
        WeaponFirer.FireWeapon(Events, damage);
    }

    private UntypedDamage toDamage(ElectricCharge charge)
    {
        return new UntypedDamage((Parameters.ChargeToDamageRate * charge.NumericValue).HitPoints());
    }
}
