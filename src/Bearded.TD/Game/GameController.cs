using Bearded.TD.Game.UI;
using Bearded.TD.Game.Units;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    class GameController
    {
        private readonly GameInstance game;

        public GameController(GameInstance game)
        {
            this.game = game;

            var blueprint = new UnitBlueprint(100, 25, new Speed(2));
            game.State.Enumerate<UnitSource>().ForEach((source) => source.QueueEnemies(blueprint, 5));
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
