using System.Collections.Generic;

namespace Bearded.TD.UI
{
    class InputState
    {
        public IReadOnlyList<char> PressedCharacters { get; }

        public InputState(IReadOnlyList<char> pressedCharacters)
        {
            PressedCharacters = pressedCharacters;
        }
    }
}