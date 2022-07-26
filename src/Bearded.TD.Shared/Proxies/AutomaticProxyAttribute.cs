using System;
using JetBrains.Annotations;

namespace Bearded.TD.Shared.Proxies;

[AttributeUsage(AttributeTargets.Interface)]
[UsedImplicitly]
public sealed class AutomaticProxyAttribute : Attribute {}
