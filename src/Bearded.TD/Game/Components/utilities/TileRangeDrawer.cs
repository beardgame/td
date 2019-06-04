using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.InGameUI;
using Bearded.TD.Tiles;
using Extensions = Bearded.TD.Tiles.Extensions;

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

            var border = TileAreaBorder.From(tilesInRange);

            TileAreaBorderRenderer.Render(border, geometries.ConsoleBackground, 
                Color.Green * (owner.SelectionState == SelectionState.Selected ? 0.5f : 0.25f)
                );
        }
    }
}
