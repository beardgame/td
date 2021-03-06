using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Movement
{
    interface IEnemyMovement : IComponent<EnemyUnit>
    {
        Position2 Position { get; }
        Tile CurrentTile { get; }
        Tile GoalTile { get; }
        bool IsMoving { get; }

        void Teleport(Position2 pos, Tile tile);
    }
}
