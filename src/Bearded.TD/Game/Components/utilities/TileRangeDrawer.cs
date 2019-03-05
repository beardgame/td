using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Components.utilities
{
    sealed class TileRangeDrawer
    {
        private readonly GameState game;
        private readonly ISelectable owner;
        private readonly Func<IEnumerable<Tile>> getTilesInRange;

        public TileRangeDrawer(GameState game, ISelectable owner, Func<IEnumerable<Tile>> getTilesInRange)
        {
            this.game = game;
            this.owner = owner;
            this.getTilesInRange = getTilesInRange;
        }
        
        public void Draw(GeometryManager geometries)
        {
            if (owner.SelectionState == SelectionState.Default) return;

            var tilesInRange = getTilesInRange();

            var geo = geometries.ConsoleBackground;

            geo.Color = Color.Green * (owner.SelectionState == SelectionState.Selected ? 0.15f : 0.1f);

            var level = game.Level;

            foreach (var tile in tilesInRange)
            {
                geo.DrawCircle(level.GetPosition(tile).NumericValue, Constants.Game.World.HexagonSide, true, 6);
            }
        }
    }
}
