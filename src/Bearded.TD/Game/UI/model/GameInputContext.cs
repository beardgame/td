using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.UI
{
    struct GameInputContext
    {
        public Position2 MousePos { get; }
        public IAction ClickAction { get; }

        private GameInputContext(Position2 mousePos, IAction clickAction)
        {
            MousePos = mousePos;
            ClickAction = clickAction;
        }

        public static GameInputContext Construct(InputState inputState, GameCamera camera)
        {
            return new GameInputContext(
                new Position2(camera.TransformScreenToWorldPos(inputState.MousePosition)), inputState.Click);
        }
    }
}
