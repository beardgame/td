using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class ParameterModifiableWithId : UpgradeEffectBase
{
    private readonly AttributeType attributeType;
    private readonly ModificationWithId modification;

    public ParameterModifiableWithId(
        AttributeType attributeType,
        ModificationWithId modification,
        UpgradePrerequisites prerequisites,
        bool isSideEffect = false
    ) : base(prerequisites, isSideEffect)
    {
        this.attributeType = attributeType;
        this.modification = modification;
    }

    public override bool CanApplyTo(IParametersTemplate subject) => subject.HasAttributeOfType(attributeType);

    public override void ApplyTo(IParametersTemplate subject)
        => subject.AddModificationWithId(attributeType, modification);

    public override bool RemoveFrom(IParametersTemplate subject)
        => subject.RemoveModification(attributeType, modification.Id);
}
