using amulware.Graphics;
using Bearded.TD.Game.Generation.Enemies;

namespace Bearded.TD.Game
{
    interface IGameController
    {
        EnemySpawnDebugParameters DebugParameters { get; }
        void Update(UpdateEventArgs args);
    }

    class DummyGameController : IGameController
    {
        public EnemySpawnDebugParameters DebugParameters => EnemySpawnDebugParameters.Empty;

        public void Update(UpdateEventArgs args)
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

        public void Update(UpdateEventArgs args)
        {
            enemySpawnController.Update(args);
        }

        public EnemySpawnDebugParameters DebugParameters => enemySpawnController.DebugParameters;
    }
}
