using System;
using System.Collections.Generic;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.World.Fluids
{
    class Fluid
    {
        private const float viscosity = 3;
        private const float maxFlowFraction = 1f / 6f;
        private static readonly Step stepR = Direction.Right.Step();
        private static readonly Step stepUR = Direction.UpRight.Step();
        private static readonly Step stepUL = Direction.UpLeft.Step();

        private readonly GeometryLayer geometryLayer;
        private readonly int radius;
        private readonly TimeSpan updateInterval;
        private Instant nextUpdate;

        private readonly Tilemap<float> amount;
        private Tilemap<Flow> currentFlow;
        private Tilemap<Flow> nextFlow;

        private readonly List<Tile> sinks = new List<Tile>();

        public Fluid(GeometryLayer geometryLayer, int radius, int updatesPerSecond)
        {
            this.geometryLayer = geometryLayer;
            this.radius = radius;
            updateInterval = new TimeSpan(1.0 / updatesPerSecond);
            nextUpdate = Instant.Zero + updateInterval;

            amount = new Tilemap<float>(radius);
            currentFlow = new Tilemap<Flow>(radius);
            nextFlow = new Tilemap<Flow>(radius);
        }

        public (Volume Volume, Fluids.Flow Flow) this[Tile tile] =>
            (new Volume(amount[tile]), currentFlow[tile].Public);

        public void Add(Tile tile, Volume volume)
        {
            DebugAssert.Argument.Satisfies(volume.NumericValue >= 0, "cannot add negative volume");
            amount[tile] += (float) volume.NumericValue;
        }

        public void AddSink(Tile tile)
        {
            DebugAssert.Argument.Satisfies(amount.IsValidTile(tile), "tile must be valid");
            sinks.Add(tile);
        }

        public void Update(Instant currentTime)
        {
            while (nextUpdate <= currentTime)
            {
                update();
                nextUpdate += updateInterval;
            }
        }

        private void update()
        {
            updateFlow();
            clampFlow();

            swapStates();

            applyFlow();

            zeroSinks();
        }

        private void updateFlow()
        {
            const float inverseViscosity = 1 / viscosity;

            for (var y = -radius; y < radius; y++)
            {
                var xMin = Math.Max(-radius + 1, -radius - y);
                var xMax = Math.Min(radius, radius - y);

                for (var x = xMin; x < xMax; x++)
                {
                    var (flowRight, flowUpRight, flowUpLeft) = currentFlow[x, y];

                    var tileAmount = waterLevel(x, y);

                    var deltaR = tileAmount - waterLevel(x + stepR.X, y + stepR.Y);
                    var deltaUR = tileAmount - waterLevel(x + stepUR.X, y + stepUR.Y);
                    var deltaUL = tileAmount - waterLevel(x + stepUL.X, y + stepUL.Y);

                    var nextTileFlow = new Flow(
                        flowRight + deltaR * inverseViscosity,
                        flowUpRight + deltaUR * inverseViscosity,
                        flowUpLeft + deltaUL * inverseViscosity
                    );

                    nextFlow[x, y] = nextTileFlow;
                }
            }
        }

        private void clampFlow()
        {
            for (var y = -radius; y < radius; y++)
            {
                var xMin = Math.Max(-radius + 1, -radius - y);
                var xMax = Math.Min(radius, radius - y);

                for (var x = xMin; x < xMax; x++)
                {
                    var (flowRight, flowUpRight, flowUpLeft) = nextFlow[x, y];

                    var clampedFlow = new Flow(
                        clampFlowComponent(flowRight, x, y, stepR),
                        clampFlowComponent(flowUpRight, x, y, stepUR),
                        clampFlowComponent(flowUpLeft, x, y, stepUL)
                    );

                    nextFlow[x, y] = clampedFlow;
                }
            }
        }

        private float clampFlowComponent(float flowCandidate, int x, int y, Step step)
        {
            if (flowCandidate > 0)
            {
                return Math.Min(flowCandidate, amount[x, y] * maxFlowFraction);
            }
            else
            {
                return -Math.Min(-flowCandidate, amount[x + step.X, y + step.Y] * maxFlowFraction);
            }
        }

        private void swapStates()
        {
            (currentFlow, nextFlow) = (nextFlow, currentFlow);
        }

        private void applyFlow()
        {
            for (var y = -radius; y < radius; y++)
            {
                var xMin = Math.Max(-radius + 1, -radius - y);
                var xMax = Math.Min(radius, radius - y);

                for (var x = xMin; x < xMax; x++)
                {
                    var (flowRight, flowUpRight, flowUpLeft) = currentFlow[x, y];

                    amount[x, y] -= flowRight + flowUpRight + flowUpLeft;

                    amount[x + stepR.X, y + stepR.Y] += flowRight;
                    amount[x + stepUR.X, y + stepUR.Y] += flowUpRight;
                    amount[x + stepUL.X, y + stepUL.Y] += flowUpLeft;
                }
            }
        }

        private void zeroSinks()
        {
            foreach (var sink in sinks)
            {
                amount[sink] = 0;
            }
        }

        private float waterLevel(int x, int y)
        {
            return amount[x, y] + geometryLayer[new Tile(x, y)]
                .DrawInfo.Height.NumericValue;
        }

        struct Flow
        {
            public float FlowRight { get; }
            public float FlowUpRight { get; }
            public float FlowUpLeft { get; }

            public Flow(float flowRight, float flowUpRight, float flowUpLeft)
            {
                FlowRight = flowRight;
                FlowUpRight = flowUpRight;
                FlowUpLeft = flowUpLeft;
            }

            public void Deconstruct(out float flowRight, out float flowUpRight, out float flowUpLeft)
            {
                flowRight = FlowRight;
                flowUpRight = FlowUpRight;
                flowUpLeft = FlowUpLeft;
            }

            public Fluids.Flow Public => new Fluids.Flow(new FlowRate(FlowRight), new FlowRate(FlowUpRight),
                new FlowRate(FlowUpLeft));
        }
    }
}
