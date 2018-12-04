using System.Collections.Generic;
using System.Linq;

namespace Bearded.TD.Shared.TechEffects
{
    public abstract class ModifiableBase
    {
        private ILookup<AttributeType, IAttributeWithModifications> attributesByType;

        protected void InitializeAttributes(
            IEnumerable<KeyValuePair<AttributeType, IAttributeWithModifications>> attributes)
        {
            attributesByType = attributes.ToLookup(attr => attr.Key, attr => attr.Value);
        }

        public bool HasAttributeOfType(AttributeType type) => attributesByType.Contains(type);

        public bool ModifyAttribute(AttributeType type, Modification modification)
        {
            var hasModified = false;
            foreach (var attribute in attributesByType[type])
            {
                hasModified = true;
                attribute.AddModification(modification);
            }

            return hasModified;
        }
    }
}
