using amulware.Graphics;
using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.UI
{
    abstract class InteractionHandler
    {
        protected GameInstance Game { get; }

        protected virtual TileSelection TileSelection { get; } = TileSelection.Single;

        protected InteractionHandler(GameInstance game)
        {
            Game = game;
        }

        public void Start(ICursorHandler cursor)
        {
            cursor.SetTileSelection(TileSelection);
            OnStart(cursor);
        }

        protected virtual void OnStart(ICursorHandler cursor) { }

        public abstract void Update(UpdateEventArgs args, GameInputContext inputContext, ICursorHandler cursor);
    }
}
