using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    sealed class NodeBlueprint : IConvertsTo<INodeBlueprint, Void>
    {
        public string? Id { get; set; }
        public List<INodeBehavior> Behaviors { get; set; } = new();

        public INodeBlueprint ToGameModel(ModMetadata modMetadata, Void _)
        {
            return new Content.Models.NodeBlueprint(
                ModAwareId.FromNameInMod(Id ?? throw new InvalidDataException(), modMetadata),
                Behaviors.Select(NodeBehaviorFactories.CreateNodeBehaviorFactory).ToImmutableArray());
        }
    }
}
