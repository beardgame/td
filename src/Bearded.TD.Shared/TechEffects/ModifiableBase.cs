using System;
using System.Collections.Generic;
using System.Linq;

namespace Bearded.TD.Shared.TechEffects
{
    public abstract class ModifiableBase<T> where T : ModifiableBase<T>
    {
        private static IDictionary<AttributeType, List<Func<T, IAttributeWithModifications>>> attributeGettersByType;

        protected static void InitializeAttributes(
            IEnumerable<KeyValuePair<AttributeType, Func<T, IAttributeWithModifications>>> attributes)
        {
            attributeGettersByType = attributes
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(kvp => kvp.Value).ToList()
                );
        }
        
        public virtual bool HasAttributeOfType(AttributeType type) => AttributeIsKnown(type);

        public virtual bool ModifyAttribute(AttributeType type, Modification modification)
            => ModifyAttributeOfInstance((T)this, type, modification);

        public static bool AttributeIsKnown(AttributeType type) => attributeGettersByType.ContainsKey(type);

        protected static bool ModifyAttributeOfInstance(T instance, AttributeType type, Modification modification)
        {
            if (!attributeGettersByType.TryGetValue(type, out var attributeGetters))
                return false;
            
            foreach (var attributeGetter in attributeGetters)
            {
                var attribute = attributeGetter(instance);
                attribute.AddModification(modification);
            }

            return true;
        }
    }
}
