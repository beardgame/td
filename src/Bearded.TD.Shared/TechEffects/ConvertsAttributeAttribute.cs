using System;
using JetBrains.Annotations;

namespace Bearded.TD.Shared.TechEffects
{
    [AttributeUsage(AttributeTargets.Field)]
    [MeansImplicitUse]
    public sealed class ConvertsAttributeAttribute : Attribute {}
}
