using System;

namespace Bearded.TD.Tiles
{
    static class LevelExtensions
    {
        public static Tile RandomTile(this Level level, Random random)
        {
            var r = level.Radius;
            while (true)
            {
                var t = new Tile(random.Next(-r, r), random.Next(-r, r));
                if (level.IsValid(t)) return t;
            }
        }
    }
}
