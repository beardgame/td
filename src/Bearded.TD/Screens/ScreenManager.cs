using System.Collections.Concurrent;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.UI;

namespace Bearded.TD.Screens
{
    class ScreenManager : ScreenLayerCollection
    {
        private readonly List<char> pressedCharacterList = new List<char>();
        private readonly ConcurrentQueue<char> pressedCharacterQueue = new ConcurrentQueue<char>();
        private readonly IReadOnlyList<char> pressedCharacterInterface;

        public ScreenManager()
        {
            pressedCharacterInterface = pressedCharacterList.AsReadOnly();
        }

        public void Update(UpdateEventArgs args)
        {
            handleInput(args);
            UpdateAll(args);
        }

        private void handleInput(UpdateEventArgs args)
        {
            char c;
            while (pressedCharacterQueue.TryDequeue(out c))
                pressedCharacterList.Add(c);

            var inputState = new InputState(pressedCharacterInterface);

            PropagateInput(args, inputState);

            pressedCharacterList.Clear();
        }

        public void RegisterPressedCharacter(char c)
        {
            pressedCharacterQueue.Enqueue(c);
        }
    }
}
