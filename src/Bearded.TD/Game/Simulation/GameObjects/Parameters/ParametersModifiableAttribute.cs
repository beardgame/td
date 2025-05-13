using System;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.GameObjects.Parameters;

[AttributeUsage(AttributeTargets.Class)]
[BaseTypeRequired(typeof(ModifiableBase<>))]
[UsedImplicitly]
public sealed class ParametersModifiableAttribute : Attribute
{
    public Type Interface { get; }

    public ParametersModifiableAttribute(Type @interface)
    {
        Interface = @interface;
    }
}
