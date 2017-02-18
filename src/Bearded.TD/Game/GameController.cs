using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    class GameController
    {
        private readonly GameState state;

        public GameController(GameState state)
        {
            this.state = state;
        }

        public void Update(TimeSpan elapsedTime, PlayerInput input)
        {
            if (input.ClickAction.Hit)
            {
                var clickedTile = state.Level.GetTile(input.MousePos);
                if (clickedTile.IsValid)
                {
                    state.Geometry.SetPassability(clickedTile, !clickedTile.Info.IsPassable);
                }
            }
        }
    }
}
