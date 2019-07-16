using System;
using JetBrains.Annotations;

namespace Bearded.TD.Shared.TechEffects
{
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    public sealed class ContainsAttributeConvertersAttribute : Attribute {}

    [AttributeUsage(AttributeTargets.Field)]
    [MeansImplicitUse]
    public sealed class ConvertsAttributeAttribute : Attribute {}
}
