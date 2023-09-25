using System.Linq;
using Bearded.UI.EventArgs;
using Bearded.Utilities.Input;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using InputAction = Bearded.Utilities.Input.InputAction;

namespace Bearded.TD.Utilities.Input;

sealed class InputState
{
    public MouseInputState Mouse { get; }
    public KeyboardInputState Keyboard { get; }

    public InputState(InputManager inputManager)
    {
        Mouse = new MouseInputState(inputManager);
        Keyboard = new KeyboardInputState(inputManager);
    }

    public ActionState ForKey(Keys key)
    {
        return Keyboard.GetKeyState(key);
    }

    public ActionState ForAnyKey(params Keys[] keys)
    {
        return Keyboard.GetAnyKeyState(keys);
    }

    public sealed class MouseInputState
    {
        private readonly IAction click;
        private readonly IAction cancel;
        private readonly IAction drag;

        public ActionState Click => new ActionState(click);
        public ActionState Cancel => new ActionState(cancel);
        public ActionState Drag => new ActionState(drag);

        public Vector2 Position { get; }
        public float DeltaScroll { get; }
        public ModifierKeys ModifierKeys { get; }

        public MouseInputState(InputManager inputManager)
        {
            click = inputManager.Actions.Mouse.LeftButton;
            cancel = inputManager.Actions.Mouse.RightButton;
            drag = inputManager.Actions.Mouse.RightButton;

            Position = inputManager.MousePosition;
            DeltaScroll = inputManager.DeltaScrollF;
            ModifierKeys = ModifierKeys.FromInputManager(inputManager);
        }
    }

    public sealed class KeyboardInputState
    {
        private readonly InputManager inputManager;

        public KeyboardInputState(InputManager inputManager)
        {
            this.inputManager = inputManager;
        }

        public ActionState GetKeyState(Keys key)
        {
            return new ActionState(inputManager.Actions.Keyboard.FromKey(key));
        }

        public ActionState GetAnyKeyState(params Keys[] keys)
        {
            return new ActionState(InputAction.AnyOf(keys.Select(key => inputManager.Actions.Keyboard.FromKey(key))));
        }
    }
}
