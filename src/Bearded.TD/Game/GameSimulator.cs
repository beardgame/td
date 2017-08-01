using amulware.Graphics;

namespace Bearded.TD.Game
{
    

    class GameSimulator : IGameSimulator
    {
        private readonly GameInstance game;

        public GameSimulator(GameInstance game)
        {
            this.game = game;
        }

        public void Update(UpdateEventArgs args)
        {
            
        }
    }

    #region Interface
    interface IGameSimulator
    {
        void Update(UpdateEventArgs args);
    }

    class DummyGameSimulator : IGameSimulator
    {
        public void Update(UpdateEventArgs args) { }
    }
    #endregion
}
