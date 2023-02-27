using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Units;

interface IEnemyMovement
{
    interface IPhysicalPresence
    {
        Position2 Position { get; }
        Tile GoalTile { get; }
    }

    bool IsMoving { get; }

    void Teleport(Position2 pos, Tile tile);
}
