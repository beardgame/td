using Bearded.TD.Game.GameState.Units;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameState.Components.Movement
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