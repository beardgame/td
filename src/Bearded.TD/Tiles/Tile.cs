using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Bearded.TD.Tiles
{
    struct Tile<TTileInfo> : IEquatable<Tile<TTileInfo>>
    {
        private readonly Tilemap<TTileInfo> tilemap;
        public int X { get; }
        public int Y { get; }

        public Tile(Tilemap<TTileInfo> tilemap, int x, int y)
        {
            this.tilemap = tilemap;
            X = x;
            Y = y;
        }

        public int Radius => X * Y >= 0
            ? Math.Abs(X + Y)
            : Math.Max(Math.Abs(X), Math.Abs(Y));
        public Vector2 Xy => new Vector2(X, Y);
        public TTileInfo Info => tilemap[this];
        public bool IsValid => tilemap?.IsValidTile(X, Y) == true;

        public Tile<TTileInfo>? ValidOrNull => IsValid ? (Tile<TTileInfo>?)this : null;

        public Tile<TTileInfo> Neighbour(Direction direction) => Offset(direction.Step());
        public Tile<TTileInfo> Offset(Step step) => new Tile<TTileInfo>(tilemap, X + step.X, Y + step.Y);

        public IEnumerable<Tile<TTileInfo>> Neighbours => this.PossibleNeighbours().Where(t => t.IsValid);

        public IEnumerable<Direction> NeighbourDirections
        {
            get
            {
                if (tilemap == null)
                    return Enumerable.Empty<Direction>();
                if (Radius < tilemap.Radius)
                    return Tilemap.Directions;
                var me = this;
                return Tilemap.Directions.Where(d => me.Neighbour(d).IsValid);
            }
        }
        public Directions NeigbourDirectionsFlags
        {
            get
            {
                if (tilemap == null)
                    return Directions.None;
                if (Radius < tilemap.Radius)
                    return Directions.All;
                var me = this;
                return Tilemap.Directions
                    .Where(d => me.Neighbour(d).IsValid)
                    .Aggregate(Directions.None, (ds, d) => ds.And(d));
            }
        }

        public int DistanceTo(Tile<TTileInfo> other)
        {
            if (Equals(other)) return 0;

            var diffHorizontal = other.X - X; // -
            var diffVertical = other.Y - Y; // \

            // Combination of \ and - can be combined into /
            var reduction = 0;
            if (Math.Sign(diffHorizontal) != Math.Sign(diffVertical))
                reduction = Math.Min(Math.Abs(diffHorizontal), Math.Abs(diffVertical));
            return Math.Abs(diffHorizontal) + Math.Abs(diffVertical) - reduction;
        }

        public override bool Equals(object obj)
            => obj is Tile<TTileInfo> tile && Equals(tile);

        public bool Equals(Tile<TTileInfo> other)
            => tilemap == other.tilemap && X == other.X && Y == other.Y;

        public override int GetHashCode() => (X * 397) ^ Y;

        public static bool operator ==(Tile<TTileInfo> t1, Tile<TTileInfo> t2) => t1.Equals(t2);
        public static bool operator !=(Tile<TTileInfo> t1, Tile<TTileInfo> t2) => !(t1 == t2);

        public override string ToString()
            => $"{X},{Y}:{ToInfoString()}";

        public string ToInfoString()
            => IsValid
                ? Info?.ToString() ?? "null"
                :"invalid";
    }
}
