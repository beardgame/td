using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Tests.Game.Generation
{
    sealed class TestNodeBlueprint : INodeBlueprint
    {
        public ModAwareId Id => ModAwareId.Invalid;
        public ImmutableArray<INodeBehaviorFactory> Behaviors => ImmutableArray<INodeBehaviorFactory>.Empty;
        public Unit? Radius => null;
        public bool Explorable => true;
    }
}
