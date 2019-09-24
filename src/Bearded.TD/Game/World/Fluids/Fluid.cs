using System;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.World.Fluids
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

        public Fluid(GeometryLayer geometryLayer, int radius, int updatesPerSecond)
        {
            this.geometryLayer = geometryLayer;
            this.radius = radius;
            updateInterval = new TimeSpan(1.0 / updatesPerSecond);
            nextUpdate = Instant.Zero + updateInterval;

            amount = new Tilemap<float>(radius);
            currentFlow = new Tilemap<Flow>(radius);
            nextFlow =  new Tilemap<Flow>(radius);
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
        }

        private void updateFlow()
        {
            var inverseViscosity = 1 / viscosity;
            
            for (var y = -radius; y < radius; y++)
            {
                var xMin = Math.Max(-radius + 1, -radius - y);
                var xMax = Math.Min(radius, radius - y);

                for (var x = xMin; x < xMax; x++)
                {
                    var flow = currentFlow[x, y];

                    var tileAmount = waterlevel(x, y);

                    var deltaR = tileAmount - waterlevel(x + stepR.X, y + stepR.Y);
                    var deltaUR = tileAmount - waterlevel(x + stepUR.X, y + stepUR.Y);
                    var deltaUL = tileAmount - waterlevel(x + stepUL.X, y + stepUL.Y);
                    
                    var nextTileFlow = new Flow(
                        flow.FlowRight + deltaR * inverseViscosity,
                        flow.FlowUpRight + deltaUR * inverseViscosity,
                        flow.FlowUpLeft + deltaUL * inverseViscosity
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
                    var flow = nextFlow[x, y];
                    
                    var clampedFlow = new Flow(
                        clampFlowComponent(flow.FlowRight, x, y, stepR),
                        clampFlowComponent(flow.FlowUpRight, x, y, stepUR),
                        clampFlowComponent(flow.FlowUpLeft, x, y, stepUL)
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
                    var flow = currentFlow[x, y];

                    amount[x, y] -= flow.FlowRight + flow.FlowUpRight + flow.FlowUpLeft;
                    
                    amount[x + stepR.X, y + stepR.Y] += flow.FlowRight;
                    amount[x + stepUR.X, y + stepUR.Y] += flow.FlowUpRight;
                    amount[x + stepUL.X, y + stepUL.Y] += flow.FlowUpLeft;
                }
            }
        }

        private float waterlevel(int x, int y)
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

            public Fluids.Flow Public => new Fluids.Flow(new FlowRate(FlowRight), new FlowRate(FlowUpRight), new FlowRate(FlowUpLeft));
        }

        public void Add(Tile tile, Volume volume)
        {
            DebugAssert.Argument.Satisfies(volume.NumericValue >= 0, "cannot add negative volume");
            
            amount[tile] += volume.NumericValue;
        }

        public (Volume Volume, Fluids.Flow Flow) this[Tile tile] => (new Volume(amount[tile]), currentFlow[tile].Public);
    }
    
    struct Flow
    {
        public FlowRate FlowRight { get; }
        public FlowRate FlowUpRight { get; }
        public FlowRate FlowUpLeft { get; }
                
        public Flow(FlowRate flowRight, FlowRate flowUpRight, FlowRate flowUpLeft)
        {
            FlowRight = flowRight;
            FlowUpRight = flowUpRight;
            FlowUpLeft = flowUpLeft;
        }

        public void Deconstruct(out FlowRate flowRight, out FlowRate flowUpRight, out FlowRate flowUpLeft)
        {
            flowRight = FlowRight;
            flowUpRight = FlowUpRight;
            flowUpLeft = FlowUpLeft;
        }
    }

}
