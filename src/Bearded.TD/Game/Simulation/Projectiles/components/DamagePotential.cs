using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("damagePotential")]
sealed class DamagePotential : Component<DamagePotential.IParameters>, IProperty<UntypedDamage>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        HitPoints Damage { get; }
    }

    public UntypedDamage Value { get; }

    public DamagePotential(IParameters parameters) : base(parameters)
    {
        Value = new UntypedDamage(parameters.Damage);
    }

    protected override void OnAdded() {}

    public override void Update(TimeSpan elapsedTime) {}
}
