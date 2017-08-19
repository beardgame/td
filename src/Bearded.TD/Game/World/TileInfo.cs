using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;

namespace Bearded.TD.Game.World
{
    class TileInfo
    {
        public enum Type : byte
        {
            Unknown = 0,
            Floor = 1,
            Wall = 2,
            Crevice = 3,
        }

        public Directions ValidDirections { get; }

        public Directions OpenDirections { get; private set; }
        public bool IsPassable => Building == null && TileType == Type.Floor;

        public Building Building { get; private set; }
        private readonly List<EnemyUnit> enemies = new List<EnemyUnit>();
        public ReadOnlyCollection<EnemyUnit> Enemies { get; }
        public Type TileType { get; private set; }

        public TileInfo(Directions validDirections, Type tileType)
        {
            ValidDirections = validDirections;
            OpenDirections = validDirections;
            TileType = tileType;
            Enemies = enemies.AsReadOnly();
        }

        public void CloseTo(Direction direction)
        {
            OpenDirections = OpenDirections.Except(direction);
        }

        public void OpenTo(Direction direction)
        {
            OpenDirections = OpenDirections.And(direction).Intersect(ValidDirections);
        }

        public void SetTileType(Type tileType)
        {
            TileType = tileType;
        }

        public void SetBuilding(Building building)
        {
            Building = building;
        }

        public void AddEnemy(EnemyUnit enemy)
        {
            enemies.Add(enemy);
        }

        public void RemoveEnemy(EnemyUnit enemy)
        {
            enemies.Remove(enemy);
        }

        public override string ToString()
            => $"{TileType}";
    }
}
