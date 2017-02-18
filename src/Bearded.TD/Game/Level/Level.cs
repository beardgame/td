using Bearded.TD.Game.Tilemap;
using Bearded.Utilities.SpaceTime;
using static System.Math;
using static Bearded.TD.Constants.Game.Level;

namespace Bearded.TD.Game.Level
{
    sealed class Level
    {
        public Tilemap<TileInfo> Tilemap { get; }

        public Level(Tilemap<TileInfo> tilemap)
        {
            Tilemap = tilemap;
        }

        public Tile<TileInfo> GetTile(Position2 position)
        {
            var yf = position.Y.NumericValue * (1 / HexagonDistanceY) + 1 / 1.5f;
            var y = Floor(yf);
            var xf = position.X.NumericValue * (1 / HexagonWidth) - y * 0.5f + 0.5f;
            var x = Floor(xf);

            var xRemainder = (xf - x) - 0.5f;
            var yRemainder = (yf - y) * 1.5f;

            var tx = (int) x;
            var ty = (int) y;

            if (xRemainder > yRemainder)
            {
                tx++;
                ty--;
            }
            else if (-xRemainder > yRemainder)
                ty--;

            return new Tile<TileInfo>(Tilemap, tx, ty);
        }

        public Position2 GetPosition(Tile<TileInfo> tile)
            => new Position2(
                (tile.X + tile.Y * 0.5f) * HexagonDistanceX,
                tile.Y * HexagonDistanceY
            );

        /*public void Draw(SpriteManager sprites)
        {
            foreach (var tile in this.tilemap)
                tile.Info.Draw();

            SurfaceManager.Instance.QueueLight(this.ambientLightSurface);

            if(this.game.DrawDebug)
            {
                var hex = sprites.EmptyHexagon;
                var font = sprites.GameText;
                font.Height = 2;

                hex.Color = Color.GrayScale(20, 0);
                font.Color = Color.White;

                var hexFull = sprites.FilledHexagon;

                var lines = sprites.Lines;
                lines.LineWidth = 0.5f;
                lines.Color = new Color(0, 50, 0, 0);

                int i = 0;
                foreach (var tile in this.tilemap)
                {
                    var position = this.GetPosition(tile);

                    if (tile.Info.OpenSides.Any())
                    {
                        // draw center piece
                        hexFull.Color = lines.Color;
                        hexFull.DrawSprite(position, 30f.Degrees().Radians, lines.LineWidth * 1.87f);

                        // show navigation graph
                        foreach (var direction in Level.activeDirections)
                        {
                            if (!tile.Info.OpenSides.Includes(direction))
                                continue;

                            var next = tile.Neighbour(direction);

                            var positionNext = this.GetPosition(next);

                            lines.DrawLine(position, positionNext);
                        }
                    }


                    var position3D = new Vector3(position.X, position.Y, Settings.Game.Level.OverlayHeight);

                    hex.DrawSprite(position3D, 0, Settings.Game.Level.HexagonDiameter);

                    font.DrawString(position3D, tile.Radius.ToString(), 0.5f, 1);
                    font.DrawString(position3D, tile.X + "," + tile.Y, 0.5f, 0);


                    i++;
                }
            }
        }*/
    }
}