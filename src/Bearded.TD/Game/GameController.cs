using Bearded.TD.Game.UI;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    class GameController
    {
        private readonly GameInstance game;
        private Position2 mousePosition;

        public GameController(GameInstance game)
        {
            this.game = game;
            game.State.Add(new Cursor(() => mousePosition));
        }

        public void Update(PlayerInput input)
        {
            mousePosition = input.MousePos;

            if (game.Cursor.ClickHandler == null) return;

            var footprint = game.Cursor.GetFootprintForPosition(input.MousePos);
            game.Cursor.Hover(footprint);
            if (input.ClickAction.Hit)
                game.Cursor.Click(footprint);
        }
    }
}
