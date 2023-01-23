using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings.Veterancy;

[Component("passiveVeterancyBonus")]
sealed class PassiveVeterancyBonus : Component<PassiveVeterancyBonus.IParameters>, IListener<GainLevel>
{
    private IUpgradeReceipt? previousUpgrade;
    private int levels;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1.15)]
        double Factor { get; }

        AttributeType? Attribute { get; }
    }

    public PassiveVeterancyBonus(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(GainLevel e)
    {
        previousUpgrade?.Rollback();
        levels++;

        var strength = Math.Pow(Parameters.Factor, levels);
        var modification = Modification.MultiplyWith(strength);
        var id = Owner.Game.GamePlayIds.GetNext<Modification>();
        var attribute = Parameters.Attribute ?? AttributeType.Damage;

        var effect =
            new ParameterModifiableWithId(
                attribute,
                new ModificationWithId(id, modification),
                UpgradePrerequisites.Empty);

        var upgrade = Upgrade.FromEffects(effect);

        previousUpgrade = Owner.ApplyUpgrade(upgrade);
    }
}

