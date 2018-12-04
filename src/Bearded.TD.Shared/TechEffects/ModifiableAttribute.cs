using System;

namespace Bearded.TD.Shared.TechEffects
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ModifiableAttribute : Attribute
    {
        public AttributeType Type { get; set; } = AttributeType.None;
        public object DefaultValue { get; }
        public Type DefaultValueType { get; set; }

        public ModifiableAttribute()
        {
        }

        public ModifiableAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }
        
        public ModifiableAttribute(object defaultValue, AttributeType type = AttributeType.None)
        {
            DefaultValue = defaultValue;
            Type = type;
        }
    }
}
