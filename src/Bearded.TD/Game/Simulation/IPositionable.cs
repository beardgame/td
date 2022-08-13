using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation;

interface IPositionable
{
    Position3 Position { get; }
}

sealed class Positionable : IPositionable
{
    public Position3 Position { get; set; }
}
