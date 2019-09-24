using Bearded.TD.Game.World;

namespace Bearded.TD.Game.Input
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

        public void End(ICursorHandler cursor)
        {
            OnEnd(cursor);
        }
        protected virtual void OnEnd(ICursorHandler cursor) { }

        public abstract void Update(ICursorHandler cursor);
    }
}
