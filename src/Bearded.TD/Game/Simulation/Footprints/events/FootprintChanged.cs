using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Simulation.Footprints;

readonly record struct FootprintChanged(PositionedFootprint NewFootprint) : IComponentEvent;
