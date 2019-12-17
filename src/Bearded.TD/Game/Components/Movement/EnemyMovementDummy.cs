using System;
using Bearded.TD.Game.Components.EnemyBehavior;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Components.Movement
{
    class EnemyMovementDummy : IEnemyMovement
    {
        public EnemyUnit Owner { get; }
        public Position2 Position { get; }
        public Tile CurrentTile { get; }

        public Tile GoalTile => CurrentTile;
        public bool IsMoving => false;

        public EnemyMovementDummy(EnemyUnit owner, Tile currentTile)
        {
            Owner = owner;
            Position = Level.GetPosition(currentTile);
            CurrentTile = currentTile;
        }
        public void OnAdded(EnemyUnit owner) => throw new NotImplementedException();
        public void Update(TimeSpan elapsedTime) => throw new NotImplementedException();
        public void Draw(GeometryManager geometries) => throw new NotImplementedException();
        public bool CanApplyUpgradeEffect(IUpgradeEffect effect) => throw new NotImplementedException();
        public void ApplyUpgradeEffect(IUpgradeEffect effect) => throw new NotImplementedException();
        public bool RemoveUpgradeEffect(IUpgradeEffect effect) => throw new NotImplementedException();
        public void Teleport(Position2 pos, Tile tile) => throw new NotImplementedException();
    }
}
