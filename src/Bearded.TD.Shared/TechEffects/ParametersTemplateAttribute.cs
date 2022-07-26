using System;
using JetBrains.Annotations;

namespace Bearded.TD.Shared.TechEffects;

[AttributeUsage(AttributeTargets.Class)]
[BaseTypeRequired(typeof(IParametersTemplate<>))]
[UsedImplicitly]
public sealed class ParametersTemplateAttribute : Attribute
{
    public Type Interface { get; }

    public ParametersTemplateAttribute(Type @interface)
    {
        Interface = @interface;
    }
}
