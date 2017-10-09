using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Utilities.Input.Actions;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Utilities.Input
{
    class InputState
    {
        public MouseInputState Mouse { get; }
        public KeyboardInputState Keyboard { get; }

        public InputState(IReadOnlyList<char> pressedCharacters, InputManager inputManager)
        {
            Mouse = new MouseInputState(inputManager);
            Keyboard = new KeyboardInputState(inputManager, pressedCharacters);
        }

        public ActionState ForKey(Key key)
        {
            return Keyboard.GetKeyState(key);
        }

        public ActionState ForAnyKey(params Key[] keys)
        {
            return Keyboard.GetAnyKeyState(keys);
        }

        public class MouseInputState : CapturableInputState
        {
            private readonly IAction click;
            private readonly IAction drag;

            public ActionState Click => GetState(click);
            public ActionState Drag => GetState(drag);

            public Vector2 Position { get; }
            public float DeltaScroll { get; }

            public MouseInputState(InputManager inputManager)
            {
                click = new MouseButtonAction(inputManager, MouseButton.Left);
                drag = new MouseButtonAction(inputManager, MouseButton.Right);

                Position = inputManager.MousePosition;
                DeltaScroll = inputManager.DeltaScrollF;
            }
        }

        public class KeyboardInputState : CapturableInputState
        {
            private readonly InputManager inputManager;
            private readonly IReadOnlyList<char> pressedCharacters;

            public KeyboardInputState(InputManager inputManager, IReadOnlyList<char> pressedCharacters)
            {
                this.inputManager = inputManager;
                this.pressedCharacters = pressedCharacters;
            }

            public ActionState GetKeyState(Key key)
            {
                return GetState(new KeyboardAction(inputManager, key));
            }

            public ActionState GetAnyKeyState(params Key[] keys)
            {
                return GetState(InputAction.AnyOf(keys.Select(key => new KeyboardAction(inputManager, key))));
            }

            public IReadOnlyList<char> PressedCharacters => IsCaptured ? new List<char>() : pressedCharacters;
        }

        internal abstract class CapturableInputState
        {
            protected bool IsCaptured { get; private set; }

            protected ActionState GetState(IAction action)
            {
                return IsCaptured ? new ActionState() : new ActionState(action);
            }

            public void Capture()
            {
                IsCaptured = true;
            }
        }
    }
}