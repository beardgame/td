using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Technologies
{
    sealed class ParameterModifiable : IUpgradeEffect
    {
        private readonly AttributeType attributeType;
        private readonly Modification modification;

        public ParameterModifiable(AttributeType attributeType, Modification modification)
        {
            this.attributeType = attributeType;
            this.modification = modification;
        }

        public bool CanApplyTo(object subject)
        {
            return subject is IParametersTemplate parametersTemplate
                && parametersTemplate.HasAttributeOfType(attributeType);
        }

        public void ApplyTo(object subject)
        {
            ((IParametersTemplate) subject).ModifyAttribute(attributeType, modification);
        }
    }
}