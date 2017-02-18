using Bearded.TD.Game.Units;
using Bearded.Utilities.Input;
using Bearded.Utilities.SpaceTime;
using OpenTK.Input;

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
                if (!clickedTile.IsValid) return;
                if (InputManager.IsKeyPressed(Key.AltLeft))
                    state.Add(new EnemyUnit(new UnitBlueprint(100, new Speed(2)), clickedTile));
                else
                    state.Geometry.ToggleTileType(clickedTile);
            }
        }
    }
}
