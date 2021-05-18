using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Generation
{
    readonly struct AccumulateNodeGroups : IGlobalEvent
    {
        private readonly Accumulator accumulator;

        public AccumulateNodeGroups(Accumulator accumulator)
        {
            this.accumulator = accumulator;
        }

        public void AddNodeGroup(NodeGroup nodeGroup)
        {
            accumulator.AddNodeGroup(nodeGroup);
        }

        public sealed class Accumulator
        {
            private readonly List<NodeGroup> nodeGroups = new();

            public void AddNodeGroup(NodeGroup nodeGroup)
            {
                nodeGroups.Add(nodeGroup);
            }

            public NodeGroup ToNodes()
            {
                if (nodeGroups.Count == 0)
                {
                    throw new InvalidDataException("No nodes were contributed in the game rules.");
                }

                return new NodeGroup.Composite(
                    nodeGroups.ToImmutableArray(), new NodeGroup.RandomizedNumber(1, null, null));
            }
        }
    }
}
