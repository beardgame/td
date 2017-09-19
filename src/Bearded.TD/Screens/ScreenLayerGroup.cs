using amulware.Graphics;
using Bearded.TD.Utilities.Input;

namespace Bearded.TD.Screens
{
    abstract class ScreenLayerGroup : ScreenLayerCollection, IScreenLayer
    {
        private readonly ScreenLayerCollection parent;

        protected ScreenLayerGroup(ScreenLayerCollection parent)
        {
            this.parent = parent;
        }

        public void Update(UpdateEventArgs args)
        {
            UpdateAll(args);
        }

        public void HandleInput(UpdateEventArgs args, InputState inputState)
        {
            PropagateInput(args, inputState);
        }

        protected void Destroy()
        {
            parent.RemoveScreenLayer(this);
        }
    }
}