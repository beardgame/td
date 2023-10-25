using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("scaleFromDamage")]
sealed class ScaleFromDamage : Component<ScaleFromDamage.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        float Factor { get; }
    }

    public ScaleFromDamage(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        ComponentDependencies.Depend<IProperty<UntypedDamage>>(Owner, Events, p =>
        {
            var scale = new Scale(p.Value.Amount.NumericValue * Parameters.Factor);
            Owner.AddComponent(new Property<Scale>(scale));
            Owner.RemoveComponent(this);
        });
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}

