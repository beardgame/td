using amulware.Graphics;
using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.UI
{
    class DefaultInteractionHandler : InteractionHandler
    {
        public DefaultInteractionHandler(GameInstance game) : base(game) { }

        public override void Start(ICursorHandler cursor)
        {
            cursor.SetTileSelection(TileSelection.FromFootprints(FootprintGroup.Single));
        }

        public override void Update(UpdateEventArgs args, ICursorHandler cursor)
        {
            
        }
    }
}
