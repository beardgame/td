using System;
using JetBrains.Annotations;

namespace Bearded.TD.Shared.Events;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse(ImplicitUseKindFlags.Access)]
public sealed class HandlerAttribute : Attribute;
