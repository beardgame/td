using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering.InGameUI;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using static Bearded.Utilities.Maybe;

namespace Bearded.TD.Game.Components.utilities
{
    sealed class TileRangeDrawer
    {
        private readonly GameState game;
        private readonly ISelectable owner;
        private readonly Func<Maybe<HashSet<Tile>>> getTilesInRange;

        public TileRangeDrawer(GameState game, ISelectable owner, Func<IEnumerable<Tile>> getTilesInRange)
            : this(game, owner, () => Just(new HashSet<Tile>(getTilesInRange())))
        {
            
        }
        
        public TileRangeDrawer(GameState game, ISelectable owner, Func<Maybe<HashSet<Tile>>> getTilesInRange)
        {
            this.game = game;
            this.owner = owner;
            this.getTilesInRange = getTilesInRange;
        }
        
        public void Draw()
        {
            if (owner.SelectionState == SelectionState.Default) return;

            getTilesInRange().Match(tiles =>
            {
                var border = TileAreaBorder.From(tiles);

                TileAreaBorderRenderer.Render(game, border,
                    Color.Green * (owner.SelectionState == SelectionState.Selected ? 0.5f : 0.25f)
                );
            });
        }
    }
}
