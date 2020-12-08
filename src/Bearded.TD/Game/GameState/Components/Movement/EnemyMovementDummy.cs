using System;
using Bearded.TD.Game.GameState.Components.Events;
using Bearded.TD.Game.GameState.Units;
using Bearded.TD.Game.GameState.Upgrades;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.GameState.Components.Movement
{
    class EnemyMovementDummy : IEnemyMovement
    {
        public Position2 Position { get; }
        public Tile CurrentTile { get; }

        public Tile GoalTile => CurrentTile;
        public bool IsMoving => false;

        public EnemyMovementDummy(Tile currentTile)
        {
            Position = Level.GetPosition(currentTile);
            CurrentTile = currentTile;
        }

        public void OnAdded(EnemyUnit owner, ComponentEvents events) => throw new InvalidOperationException();
        public void Update(TimeSpan elapsedTime) => throw new InvalidOperationException();
        public void Draw(GeometryManager geometries) => throw new InvalidOperationException();
        public bool CanApplyUpgradeEffect(IUpgradeEffect effect) => throw new InvalidOperationException();
        public void ApplyUpgradeEffect(IUpgradeEffect effect) => throw new InvalidOperationException();
        public bool RemoveUpgradeEffect(IUpgradeEffect effect) => throw new InvalidOperationException();
        public void Teleport(Position2 pos, Tile tile) => throw new InvalidOperationException();
    }
}