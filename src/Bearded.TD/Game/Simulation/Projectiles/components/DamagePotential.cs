using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("damagePotential")]
sealed class DamagePotential : Component<DamagePotential.IParameters>, IProperty<DamageInfo>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        HitPoints Damage { get; }
        DamageType? Type { get; }
    }

    public DamageInfo Value { get; }

    public DamagePotential(IParameters parameters) : base(parameters)
    {
        Value = new DamageInfo(parameters.Damage, parameters.Type ?? DamageType.Kinetic);
    }

    protected override void OnAdded() {}

    public override void Update(TimeSpan elapsedTime) {}
}
