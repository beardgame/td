using System;

namespace Bearded.TD.Utilities;

static class Functions<T>
{
    public static Func<T, bool> AlwaysTrue { get; } = _ => true;
}
