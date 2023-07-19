using System;
using JetBrains.Annotations;

namespace Bearded.TD.Utilities.Console;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
sealed class CommandParameterCompletionAttribute : Attribute
{
    public string Name { get; }

    public CommandParameterCompletionAttribute(string name)
    {
        Name = name;
    }
}