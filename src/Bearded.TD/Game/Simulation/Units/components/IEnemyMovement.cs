using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Units;

interface IEnemyMovement
{
    bool IsMoving { get; }

    Direction TileDirection { get; }
}
