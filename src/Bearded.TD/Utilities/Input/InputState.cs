using System.Collections.Generic;
using Bearded.TD.Utilities.Input.Actions;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Utilities.Input
{
    class InputState
    {
        private readonly InputManager inputManager;

        public IReadOnlyList<char> PressedCharacters { get; }
        public Vector2 MousePosition => inputManager.MousePosition;
        public IAction Click { get; }

        public InputState(IReadOnlyList<char> pressedCharacters, InputManager inputManager)
        {
            PressedCharacters = pressedCharacters;
            this.inputManager = inputManager;

            Click = new MouseButtonAction(inputManager, MouseButton.Left);
        }

        public IAction ForKey(Key key)
        {
            return new KeyboardAction(inputManager, key);
        }
    }
}