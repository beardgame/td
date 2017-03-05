using System.Collections.Concurrent;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;

namespace Bearded.TD.Screens
{
    class ScreenManager
    {
        private readonly List<ScreenLayer> screenLayers = new List<ScreenLayer>();

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

            screenLayers.ForEach((layer) => layer.Update(args));
        }

        private void handleInput(UpdateEventArgs args)
        {
            char c;
            while (pressedCharacterQueue.TryDequeue(out c))
                pressedCharacterList.Add(c);

            var inputState = new InputState(pressedCharacterInterface);

            for (var i = screenLayers.Count - 1; i >= 0; i--)
                if (!screenLayers[i].HandleInput(args, inputState))
                    break;

            pressedCharacterList.Clear();
        }

        public void Draw(RenderContext context)
        {
            screenLayers.ForEach(context.Compositor.RenderLayer);
        }

        public void OnResize(ViewportSize newSize)
        {
            screenLayers.ForEach((layer) => layer.OnResize(newSize));
        }

        public void AddScreenLayer(ScreenLayer screenLayer)
        {
            screenLayers.Add(screenLayer);
        }

        public void RegisterPressedCharacter(char c)
        {
            pressedCharacterQueue.Enqueue(c);
        }
    }
}
