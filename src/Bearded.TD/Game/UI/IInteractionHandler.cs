using amulware.Graphics;

namespace Bearded.TD.Game.UI
{
    abstract class InteractionHandler
    {
        protected GameInstance Game { get; }

        protected InteractionHandler(GameInstance game)
        {
            Game = game;
        }

        public abstract void Start(ICursorHandler cursor);
        public abstract void Update(UpdateEventArgs args, GameInputContext inputContext, ICursorHandler cursor);
    }
}
