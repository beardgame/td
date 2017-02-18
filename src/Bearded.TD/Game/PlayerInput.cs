using Bearded.Utilities.Input;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Game
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

        public static PlayerInput Construct(GameCamera camera)
        {
            return new PlayerInput(
                new Position2(camera.TransformScreenToWorldPos(InputManager.MousePosition)),
                MouseAction.ForLeftButton);
        }
    }
}
