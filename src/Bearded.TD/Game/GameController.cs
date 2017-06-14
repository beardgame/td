using Bearded.TD.Game.UI;
using Bearded.TD.Game.Units;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    class GameController
    {
        private static readonly UnitBlueprint debugBlueprint = new UnitBlueprint(100, 25, new Speed(2), 10);
        private static readonly TimeSpan timeBetweenWaves = new TimeSpan(15);

        private readonly GameInstance game;
        private Instant nextWave;

        public GameController(GameInstance game)
        {
            this.game = game;

            queueEnemyWave();
            nextWave = game.State.Time + timeBetweenWaves;
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

            if (nextWave <= game.State.Time)
            {
                queueEnemyWave();
                nextWave += timeBetweenWaves;
            }
        }

        private void queueEnemyWave()
        {
            var source = game.State.Enumerate<UnitSource>().RandomElement();
            source.QueueEnemies(debugBlueprint, StaticRandom.Int(5, 10));
        }
    }
}
