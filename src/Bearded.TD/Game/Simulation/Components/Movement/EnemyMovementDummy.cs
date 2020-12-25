using System;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components.Movement
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
        public void Draw(CoreDrawers geometries) => throw new InvalidOperationException();
        public bool CanApplyUpgradeEffect(IUpgradeEffect effect) => throw new InvalidOperationException();
        public void ApplyUpgradeEffect(IUpgradeEffect effect) => throw new InvalidOperationException();
        public bool RemoveUpgradeEffect(IUpgradeEffect effect) => throw new InvalidOperationException();
        public void Teleport(Position2 pos, Tile tile) => throw new InvalidOperationException();
    }
}
