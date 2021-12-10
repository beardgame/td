using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation.Semantic.Features;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class NodeGroupConverter : JsonConverterBase<NodeGroup>
{
    private readonly IDependencyResolver<INodeBlueprint> nodeResolver;

    public NodeGroupConverter(IDependencyResolver<INodeBlueprint> nodeResolver)
    {
        this.nodeResolver = nodeResolver;
    }

    protected override NodeGroup ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var jsonModel = serializer.Deserialize<JsonModel>(reader);
        return createNodeGroup(jsonModel);
    }

    private NodeGroup toCompositeNodeGroup(
        IEnumerable<JsonModel> nodes, NodeGroup.NumberRestriction numberRestriction)
    {
        return new NodeGroup.Composite(
            nodes.Select(createNodeGroup).ToImmutableArray(), numberRestriction);
    }

    private NodeGroup createNodeGroup(JsonModel jsonModel)
    {
        var idSet = jsonModel.Id != null;
        var childrenSet = (jsonModel.Nodes?.Count ?? 0) > 0;

        if (idSet == childrenSet)
        {
            throw new InvalidDataException("Must specify exactly one of id or nodes on a node group.");
        }

        var numberRestriction = createNumberRestriction(jsonModel);

        if (childrenSet)
        {
            return toCompositeNodeGroup(jsonModel.Nodes!, numberRestriction);
        }

        return new NodeGroup.Leaf(nodeResolver.Resolve(jsonModel.Id), numberRestriction);
    }

    private static NodeGroup.NumberRestriction createNumberRestriction(JsonModel jsonModel)
    {
        if (jsonModel.FixedNumber != null)
        {
            if (jsonModel.MinNumber != null || jsonModel.MaxNumber != null || jsonModel.Weight != null)
            {
                throw new InvalidDataException(
                    "If fixed number is set on a node, min number, max number, and weight may not be set.");
            }

            return new NodeGroup.FixedNumber(jsonModel.FixedNumber.Value);
        }

        if (jsonModel.Weight == null)
        {
            throw new InvalidDataException("Weight must always be set if a fixed number isn't set.");
        }

        return new NodeGroup.RandomizedNumber(jsonModel.Weight.Value, jsonModel.MinNumber, jsonModel.MaxNumber);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class JsonModel
    {
        // One of these is expected to be set.
        public string? Id { get; set; }
        public List<JsonModel>? Nodes { get; set; }

        public int? FixedNumber { get; set; }
        public int? MinNumber { get; set; }
        public int? MaxNumber { get; set; }

        public int? Weight { get; set; }
    }
}