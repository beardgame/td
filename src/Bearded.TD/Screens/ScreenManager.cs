using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;

namespace Bearded.TD.Screens
{
    class ScreenManager
    {
        private readonly List<ScreenLayer> screenLayers = new List<ScreenLayer>();

        public void Update(UpdateEventArgs args)
        {
            for (var i = screenLayers.Count - 1; i >= 0; i--)
                if (!screenLayers[i].HandleInput(args))
                    break;
            screenLayers.ForEach((layer) => layer.Update(args));
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
    }
}
