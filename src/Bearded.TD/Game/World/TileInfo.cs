using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Units;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.World
{
    class TileInfo
    {
        public static TileInfo Dummy { get; }

        public enum Type : byte
        {
            Unknown = 0,
            Floor = 1,
            Wall = 2,
            Crevice = 3,
        }

        [Flags]
        public enum PassabilityLayer : byte
        {
            None = 0,
            Building = 1 << 0,
            Unit = 1 << 1,
            Flying = 1 << 2,
            Worker = 1 << 3,
            Projectile = 1 << 4,

            All = 0xff,
        }

        private static readonly IDictionary<Type, PassabilityLayer> blockedLayersByType;

        // Use static constructor, because order of initialization of static members matters.
        static TileInfo()
        {
            blockedLayersByType = new Dictionary<Type, PassabilityLayer>
            {
                { Type.Unknown, PassabilityLayer.None },
                { Type.Floor, PassabilityLayer.None },
                { Type.Wall, PassabilityLayer.All },
                { Type.Crevice, ~(PassabilityLayer.Flying | PassabilityLayer.Projectile) },
            };

            Dummy = new TileInfo(Directions.None, Type.Unknown);
        }

        public Directions ValidDirections { get; }
        public Directions OpenDirectionsForUnits { get; private set; }
        private Type tileType;

        private IPlacedBuilding placedBuilding;
        public IPlacedBuilding PlacedBuilding
        {
            get => placedBuilding;
            set
            {
                placedBuilding = value;
                updatePassability();
            }
        }

        private Building finishedBuilding;
        public Building FinishedBuilding
        {
            get => finishedBuilding;
            set
            {
                finishedBuilding = value;
                updatePassability();
            }
        }

        private readonly List<EnemyUnit> enemies = new List<EnemyUnit>();
        public ReadOnlyCollection<EnemyUnit> Enemies { get; }

        public TileDrawInfo DrawInfo { get; private set; }

        private bool blockedForBuilding;
        private PassabilityLayer blockedFor;

        public TileInfo(Directions validDirections, Type tileType)
        {
            ValidDirections = validDirections;
            OpenDirectionsForUnits = validDirections;
            this.tileType = tileType;
            Enemies = enemies.AsReadOnly();
            updatePassability();
        }

        public void SetTileType(Type newType)
        {
            tileType = newType;
            updatePassability();
        }

        public void CloseForUnitsTo(Direction direction)
        {
            OpenDirectionsForUnits = OpenDirectionsForUnits.Except(direction);
        }

        public void OpenForUnitsTo(Direction direction)
        {
            OpenDirectionsForUnits = OpenDirectionsForUnits.And(direction).Intersect(ValidDirections);
        }

        public void BlockForBuilding()
        {
            blockedForBuilding = true;
            updatePassability();
        }

        public void SetDrawInfo(TileDrawInfo info)
        {
            DrawInfo = info;
        }

        public void AddEnemy(EnemyUnit enemy)
        {
            enemies.Add(enemy);
        }

        public void RemoveEnemy(EnemyUnit enemy)
        {
            enemies.Remove(enemy);
        }

        public bool IsMineable => tileType == Type.Wall;

        public bool HasKnownType => tileType != Type.Unknown;

        public bool IsPassableFor(PassabilityLayer passabilityLayer)
            => (passabilityLayer & blockedFor) == PassabilityLayer.None;

        private void updatePassability()
        {
            blockedFor = blockedLayersByType[tileType];

            if (FinishedBuilding != null)
            {
                blockedFor |= ~(PassabilityLayer.Worker | PassabilityLayer.Projectile);
            }

            if (PlacedBuilding != null || blockedForBuilding)
            {
                blockedFor |= PassabilityLayer.Building;
            }
        }

        public override string ToString() => $"{tileType}";
    }
}
