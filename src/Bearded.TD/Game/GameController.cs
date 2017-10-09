using Bearded.TD.Game.Generation.Enemies;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    interface IGameController
    {
        EnemySpawnDebugParameters DebugParameters { get; }
        void Update(TimeSpan elapsedTime);
    }

    class DummyGameController : IGameController
    {
        public EnemySpawnDebugParameters DebugParameters => EnemySpawnDebugParameters.Empty;

        public void Update(TimeSpan elapsedTime)
        {
        }
    }

    class GameController : IGameController
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

        public EnemySpawnDebugParameters DebugParameters => enemySpawnController.DebugParameters;
    }
}
