using System;

namespace Bearded.TD.Shared.TechEffects;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ModifiableAttribute : Attribute
{
    public AttributeType Type { get; set; } = AttributeType.None;
    public object DefaultValue { get; }
    public Type DefaultValueType { get; set; }

    // Important note: there is currently no constructor which takes Type as optional parameters.
    // The weaver is dependent on the Type being said through "Type = AttributeType.Something" for some reason.

    public ModifiableAttribute()
    {
    }

    public ModifiableAttribute(object defaultValue)
    {
        DefaultValue = defaultValue;
    }
}
