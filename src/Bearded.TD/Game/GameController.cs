using amulware.Graphics;

namespace Bearded.TD.Game
{
    class GameController : IGameController
    {
        private readonly GameInstance game;

        public GameController(GameInstance game)
        {
            this.game = game;
        }

        public void Update(UpdateEventArgs args)
        {
            
        }
    }

    #region Interface
    interface IGameController
    {
        void Update(UpdateEventArgs args);
    }

    class DummyGameController : IGameController
    {
        public void Update(UpdateEventArgs args) { }
    }
    #endregion
}
