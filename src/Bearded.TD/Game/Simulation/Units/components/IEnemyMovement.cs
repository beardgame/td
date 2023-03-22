using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Units;

interface IEnemyMovement
{
    bool IsMoving { get; }

    Direction TileDirection { get; }
}
