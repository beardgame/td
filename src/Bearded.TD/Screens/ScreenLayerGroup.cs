using amulware.Graphics;
using Bearded.TD.UI;

namespace Bearded.TD.Screens
{
    abstract class ScreenLayerGroup : ScreenLayerCollection, IScreenLayer
    {
        public void Update(UpdateEventArgs args)
        {
            UpdateAll(args);
        }

        public bool HandleInput(UpdateEventArgs args, InputState inputState)
        {
            return PropagateInput(args, inputState);
        }
    }
}