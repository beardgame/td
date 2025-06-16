using System;
using JetBrains.Annotations;

namespace Bearded.TD.Shared.Commands;

[AttributeUsage(AttributeTargets.Field)]
[MeansImplicitUse]
public sealed class SerializerConverterAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public sealed class SerializerAttribute : Attribute;
