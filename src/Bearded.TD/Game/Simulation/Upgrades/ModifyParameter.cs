using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class ModifyParameter : UpgradeEffectBase
{
    private readonly AttributeType attributeType;
    private readonly Modification modification;

    public ModifyParameter(
        AttributeType attributeType, Modification modification, UpgradePrerequisites prerequisites, bool isSideEffect
    ) : base(prerequisites, isSideEffect)
    {
        this.attributeType = attributeType;
        this.modification = modification;
    }

    public override bool CanApplyTo(IParametersTemplate subject) => subject.HasAttributeOfType(attributeType);

    public override void ApplyTo(IParametersTemplate subject)
        => subject.AddModification(attributeType, modification);
}
