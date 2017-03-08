using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;

namespace Bearded.TD.Screens
{
    class ScreenLayerCollection
    {
        private readonly List<IScreenLayer> screenLayers = new List<IScreenLayer>();

        protected bool PropagateInput(UpdateEventArgs args, InputState inputState)
        {
            for (var i = screenLayers.Count - 1; i >= 0; i--)
                if (!screenLayers[i].HandleInput(args, inputState))
                    return false;
            return true;
        }
        
        protected void UpdateAll(UpdateEventArgs args)
        {
            screenLayers.ForEach((layer) => layer.Update(args));
        }

        public void OnResize(ViewportSize newSize)
        {
            screenLayers.ForEach((layer) => layer.OnResize(newSize));
        }

        public void Render(RenderContext context)
        {
            screenLayers.ForEach((layer) => layer.Render(context));
        }

        public void AddScreenLayer(IScreenLayer screenLayer)
        {
            screenLayers.Add(screenLayer);
        }
    }
}