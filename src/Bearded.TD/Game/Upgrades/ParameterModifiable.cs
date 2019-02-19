using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Upgrades
{
    sealed class ParameterModifiable : UpgradeEffectBase
    {
        private readonly AttributeType attributeType;
        private readonly Modification modification;

        public ParameterModifiable(AttributeType attributeType, Modification modification)
        {
            this.attributeType = attributeType;
            this.modification = modification;
        }

        public override bool CanApplyTo<T>(IParametersTemplate<T> subject) => subject.HasAttributeOfType(attributeType);
        
        public override void ApplyTo<T>(IParametersTemplate<T> subject)
            => subject.AddModification(attributeType, modification);
    }
}
