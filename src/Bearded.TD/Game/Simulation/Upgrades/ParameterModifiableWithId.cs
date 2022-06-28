using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class ParameterModifiableWithId : UpgradeEffectBase
{
    private readonly AttributeType attributeType;
    private readonly ModificationWithId modification;

    public ParameterModifiableWithId(
        AttributeType attributeType, ModificationWithId modification, UpgradePrerequisites? prerequisites)
        : base(prerequisites)
    {
        this.attributeType = attributeType;
        this.modification = modification;
    }

    public override bool CanApplyTo<T>(IParametersTemplate<T> subject) => subject.HasAttributeOfType(attributeType);

    public override void ApplyTo<T>(IParametersTemplate<T> subject)
        => subject.AddModificationWithId(attributeType, modification);

    public override bool RemoveFrom<T>(IParametersTemplate<T> subject)
        => subject.RemoveModification(attributeType, modification.Id);
}
