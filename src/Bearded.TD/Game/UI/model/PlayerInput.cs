using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.UI
{
    struct PlayerInput
    {
        public Position2 MousePos { get; }
        public IAction ClickAction { get; }

        private PlayerInput(Position2 mousePos, IAction clickAction)
        {
            MousePos = mousePos;
            ClickAction = clickAction;
        }

        public static PlayerInput Construct(InputState inputState, GameCamera camera)
        {
            return new PlayerInput(
                new Position2(camera.TransformScreenToWorldPos(inputState.MousePosition)), inputState.Click);
        }
    }
}
