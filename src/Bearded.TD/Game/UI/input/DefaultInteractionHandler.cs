using amulware.Graphics;

namespace Bearded.TD.Game.UI
{
    class DefaultInteractionHandler : InteractionHandler
    {
        public DefaultInteractionHandler(GameInstance game) : base(game) { }

        public override void Update(UpdateEventArgs args, ICursorHandler cursor)
        {
            // Set hover state of thing below cursor.
        }
    }
}
