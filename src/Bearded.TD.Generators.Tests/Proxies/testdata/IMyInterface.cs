using System.Collections.Immutable;
using Bearded.TD.Shared.Proxies;
using JetBrains.Annotations;

namespace Bearded.TD.Generators.Tests.Proxies
{
    [AutomaticProxy]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    interface IMyInterface
    {
        int IntProperty { get; }
        ImmutableArray<int> ImmutableArrayProperty { get; }
    }
}
