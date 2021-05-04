using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Rules;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    sealed class GameModeBlueprint : IConvertsTo<IGameModeBlueprint, GameModeBlueprint.DependencyResolvers>
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public List<IGameRule> Rules { get; set; } = new();
        public List<NodeGroup> LevelNodes { get; set; } = new();

        public IGameModeBlueprint ToGameModel(ModMetadata modMetadata, DependencyResolvers resolvers)
        {
            var nodeGroupFactory = new NodeGroupFactory(resolvers);
            return new Content.Models.GameModeBlueprint(
                ModAwareId.FromNameInMod(Id ?? throw new InvalidDataException("Id must be non-null"), modMetadata),
                Name ?? throw new InvalidDataException("Name must be non-null"),
                Rules.Select(GameRuleFactories.CreateGameRuleFactory<GameState>),
                nodeGroupFactory.Create(LevelNodes));
        }

        public record DependencyResolvers(IDependencyResolver<INodeBlueprint> NodeResolver);

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public sealed class NodeGroup
        {
            // One of these is expected to be set.
            public string? Id { get; set; }
            public List<NodeGroup>? Nodes { get; set; }

            public int? FixedNumber { get; set; }
            public int? MinNumber { get; set; }
            public int? MaxNumber { get; set; }

            public int? Weight { get; set; }
        }

        private class NodeGroupFactory
        {
            private readonly DependencyResolvers dependencyResolvers;

            public NodeGroupFactory(DependencyResolvers dependencyResolvers)
            {
                this.dependencyResolvers = dependencyResolvers;
            }

            public Content.Models.NodeGroup Create(IEnumerable<NodeGroup> nodes) =>
                toCompositeNodeGroup(nodes, new Content.Models.NodeGroup.FixedNumber(1));

            private Content.Models.NodeGroup toCompositeNodeGroup(
                IEnumerable<NodeGroup> nodes, Content.Models.NodeGroup.NumberRestriction numberRestriction)
            {
                return new Content.Models.NodeGroup.Composite(
                    nodes.Select(createNodeGroup).ToImmutableArray(), numberRestriction);
            }

            private Content.Models.NodeGroup createNodeGroup(NodeGroup node)
            {
                var idSet = node.Id != null;
                var childrenSet = (node.Nodes?.Count ?? 0) > 0;

                if (idSet == childrenSet)
                {
                    throw new InvalidDataException("Must specify exactly one of id or nodes on a node group.");
                }

                var numberRestriction = createNumberRestriction(node);

                if (childrenSet)
                {
                    return toCompositeNodeGroup(node.Nodes!, numberRestriction);
                }

                return new Content.Models.NodeGroup.Leaf(
                    dependencyResolvers.NodeResolver.Resolve(node.Id), numberRestriction);
            }

            private static Content.Models.NodeGroup.NumberRestriction createNumberRestriction(NodeGroup node)
            {
                if (node.FixedNumber != null)
                {
                    if (node.MinNumber != null || node.MaxNumber != null || node.Weight != null)
                    {
                        throw new InvalidDataException(
                            "If fixed number is set on a node, min number, max number, and weight may not be set.");
                    }

                    return new Content.Models.NodeGroup.FixedNumber(node.FixedNumber.Value);
                }

                if (node.Weight == null)
                {
                    throw new InvalidDataException("Weight must always be set if a fixed number isn't set.");
                }

                return new Content.Models.NodeGroup.RandomizedNumber(node.Weight.Value, node.MinNumber, node.MaxNumber);
            }
        }
    }
}
