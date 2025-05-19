using System;
using JetBrains.Annotations;

namespace Bearded.TD.Utilities.Console;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
class CommandAttribute(string name, params string[] parameterCompletions) : Attribute
{
    public string Name { get; } = name;
    public string[] ParameterCompletions { get; } = parameterCompletions;
}

[AttributeUsage(AttributeTargets.Method)]
sealed class DebugCommandAttribute(string name, params string[] parameterCompletions)
    : CommandAttribute(name, parameterCompletions);
