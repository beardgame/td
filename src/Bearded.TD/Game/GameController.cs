using Bearded.TD.Game.UI;

namespace Bearded.TD.Game
{
    class GameController
    {
        private readonly GameInstance game;

        public GameController(GameInstance game)
        {
            this.game = game;
        }

        public void Update(PlayerInput input)
        {
            if (game.Cursor.ClickHandler != null)
            {
                var footprint = game.Cursor.GetFootprintForPosition(input.MousePos);
                game.Cursor.Hover(footprint);
                if (input.ClickAction.Hit)
                    game.Cursor.Click(footprint);
            }
        }
    }
}
