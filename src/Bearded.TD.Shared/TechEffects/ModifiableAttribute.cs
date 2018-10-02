﻿using System;

namespace Bearded.TD.Shared.TechEffects
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ModifiableAttribute : Attribute
    {
        public ModificationType Type { get; set; } = ModificationType.None;
        public object DefaultValue { get; }
        public Type DefaultValueType { get; set; }

        public ModifiableAttribute()
        {
        }

        public ModifiableAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }
    }
}
