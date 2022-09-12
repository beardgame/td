using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game.Generation.Semantic.Logical;

sealed class DeterministicNodeChooser
{
    private readonly Logger logger;

    public DeterministicNodeChooser(Logger logger)
    {
        this.logger = logger;
    }

    public IEnumerable<Node> ChooseNodes(NodeGroup nodes, int nodeCount)
    {
        switch (nodes)
        {
            case NodeGroup.Leaf leaf:
                return Enumerable.Range(0, nodeCount).Select(_ => Node.FromBlueprint(leaf.Blueprint));
            case NodeGroup.Composite composite:
                var nodeCounts = distributeNodeCount(composite, nodeCount);
                return composite.Children.SelectMany(node => ChooseNodes(node, nodeCounts[node]));
            default:
                throw new ArgumentException("Nodes must be either leaf or composite.", nameof(nodes));
        }
    }

    private IDictionary<NodeGroup, int> distributeNodeCount(NodeGroup.Composite composite, int nodeCount)
    {
        var nodes = composite.Children;

        var result = ImmutableDictionary.CreateBuilder<NodeGroup, int>();
        result.AddRange(nodes.Select(n => new KeyValuePair<NodeGroup, int>(n, 0)));

        var assignedCount = 0;

        // 1. Assign fixed and minimum numbers.
        foreach (var n in nodes)
        {
            var number = n.Number switch
            {
                NodeGroup.FixedNumber fixedNumber => fixedNumber.Number,
                NodeGroup.RandomizedNumber randomizedNumber => randomizedNumber.MinNumber ?? 0,
                _ => 0,
            };
            if (number == 0 || tryAddNodeCount(n, number))
            {
                continue;
            }

            logger.Warning?.Log($"Did not have enough nodes to satisfy constraints for {n} inside {composite}");
            return result.ToImmutable();
        }

        if (assignedCount == nodeCount)
        {
            return result.ToImmutable();
        }

        // 2. Assign the remaining nodes using the D'Hondt method.
        var flexibleNodes = nodes
            .Where(n => n.Number is NodeGroup.RandomizedNumber)
            .Select(n => new NodeAllocationQuotient(
                n, (NodeGroup.RandomizedNumber) n.Number, result.ContainsKey(n) ? result[n] : 0))
            .ToList();

        var allFlexibleNodes = flexibleNodes.ToImmutableArray();
        var eligibleFlexibleNodes = flexibleNodes.Where(n => n.CanIncrease).ToList();
        var ignoreMaximums = false;

        while (assignedCount < nodeCount)
        {
            if (!ignoreMaximums && eligibleFlexibleNodes.Count == 0)
            {
                logger.Warning?.Log(
                    $"Tried to distribute nodes, but max number reached for all groups in {composite}. " +
                    "Ignoring maximums.");
                ignoreMaximums = true;
            }

            IEnumerable<NodeAllocationQuotient> nodesToChooseFrom =
                ignoreMaximums ? allFlexibleNodes : eligibleFlexibleNodes;
            var nodeToIncrease = nodesToChooseFrom.MaxBy(n => n.Quotient);
            nodeToIncrease.Count++;

            if (!ignoreMaximums && !nodeToIncrease.CanIncrease)
            {
                eligibleFlexibleNodes.Remove(nodeToIncrease);
            }

            assignedCount++;
        }

        foreach (var n in allFlexibleNodes.Where(node => node.Count > 0))
        {
            result[n.Node] = n.Count;
        }

        return result.ToImmutable();

        bool tryAddNodeCount(NodeGroup n, int count)
        {
            if (count > nodeCount - assignedCount)
            {
                tryAddNodeCount(n, nodeCount - assignedCount);
                return false;
            }

            result[n] += count;
            assignedCount += count;

            return true;
        }
    }

    private sealed class NodeAllocationQuotient
    {
        public NodeGroup Node { get; }

        private readonly NodeGroup.RandomizedNumber number;

        private int count;

        public int Count
        {
            get => count;
            set
            {
                count = value;
                Quotient = calculateQuotient();
            }
        }
        public double Quotient { get; private set; }

        public bool CanIncrease => number.MaxNumber == null || Count < number.MaxNumber.Value;

        public NodeAllocationQuotient(NodeGroup node, NodeGroup.RandomizedNumber number, int count)
        {
            Node = node;
            this.number = number;
            Count = count;
        }

        private double calculateQuotient() => number.Weight / (Count + 1);
    }
}
