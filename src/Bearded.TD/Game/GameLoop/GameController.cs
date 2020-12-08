using Bearded.TD.Game.Generation.Enemies;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameLoop
{
    interface IGameController
    {
        void Update(TimeSpan elapsedTime);
    }

    sealed class DummyGameController : IGameController
    {
        public void Update(TimeSpan elapsedTime)
        {
        }
    }

    sealed class GameController : IGameController
    {
        private readonly EnemySpawnController enemySpawnController;

        public GameController(GameInstance game)
        {
            enemySpawnController = new EnemySpawnController(game);
        }

        public void Update(TimeSpan elapsedTime)
        {
            enemySpawnController.Update(elapsedTime);
        }
    }
}
