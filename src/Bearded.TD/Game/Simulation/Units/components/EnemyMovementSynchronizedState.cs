using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Units;

sealed class EnemyMovementSynchronizedState : ISynchronizedState<IEnemyMovement>
{
    private float positionX;
    private float positionY;
    private int goalTileX;
    private int goalTileY;

    public EnemyMovementSynchronizedState(IEnemyMovement enemyMovement)
    {
        positionX = enemyMovement.Position.X.NumericValue;
        positionY = enemyMovement.Position.Y.NumericValue;
        goalTileX = enemyMovement.GoalTile.X;
        goalTileY = enemyMovement.GoalTile.Y;
    }

    public void ApplyTo(IEnemyMovement subject)
    {
        subject.Teleport(new Position2(positionX, positionY), new Tile(goalTileX, goalTileY));
    }

    public void Serialize(INetBufferStream stream)
    {
        stream.Serialize(ref positionX);
        stream.Serialize(ref positionY);
        stream.Serialize(ref goalTileX);
        stream.Serialize(ref goalTileY);
    }
}