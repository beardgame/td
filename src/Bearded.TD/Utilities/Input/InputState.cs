using System.Collections.Generic;

namespace Bearded.TD.Utilities.Input
{
    class InputState
    {
        public IReadOnlyList<char> PressedCharacters { get; }
        public InputManager InputManager { get; }

        public InputState(IReadOnlyList<char> pressedCharacters, InputManager inputManager)
        {
            PressedCharacters = pressedCharacters;
            InputManager = inputManager;
        }
    }
}