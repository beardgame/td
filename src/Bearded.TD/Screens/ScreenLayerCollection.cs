using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;

namespace Bearded.TD.Screens
{
    class ScreenLayerCollection
    {
        private readonly List<IScreenLayer> screenLayers = new List<IScreenLayer>();
        private readonly HashSet<IScreenLayer> screenLayersToRemove = new HashSet<IScreenLayer>();
        private ViewportSize viewportSize;

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
            removeScreenLayersQueuedForRemoval();
        }

        public void OnResize(ViewportSize newSize)
        {
            viewportSize = newSize;
            screenLayers.ForEach((layer) => layer.OnResize(newSize));
        }

        public void Render(RenderContext context)
        {
            screenLayers.ForEach((layer) => layer.Render(context));
        }

        public void AddScreenLayerOnTop(IScreenLayer screenLayer)
        {
            screenLayers.Add(screenLayer);
            initializeScreenLayer(screenLayer);
        }

        private void addScreenLayerAtIndex(int index, IScreenLayer screenLayer)
        {
            screenLayers.Insert(index, screenLayer);
            initializeScreenLayer(screenLayer);
        }

        public void AddScreenLayerOnTopOf(IScreenLayer reference, IScreenLayer toAdd)
        {
            // Equivalent to adding screen layer _after_ reference.
            var indexOfRef = screenLayers.IndexOf(reference);
            if (indexOfRef == screenLayers.Count)
                AddScreenLayerOnTop(toAdd);
            else
                addScreenLayerAtIndex(indexOfRef + 1, toAdd);
        }

        public void AddScreenLayerBehind(IScreenLayer reference, IScreenLayer toAdd)
        {
            // Equivalent to adding screen layer _before_ reference.
            addScreenLayerAtIndex(screenLayers.IndexOf(reference), toAdd);
        }

        private void initializeScreenLayer(IScreenLayer screenLayer)
        {
            screenLayer.OnResize(viewportSize);
        }

        public void RemoveScreenLayer(IScreenLayer screenLayer)
        {
            // If you delete a screen layer during the HandleInput method or outside of the main loop entirely, another
            // update may take place before the screen is actually deleted.
            screenLayersToRemove.Add(screenLayer);
        }

        private void removeScreenLayersQueuedForRemoval()
        {
            if (screenLayersToRemove.Count == 0)
                return;
            screenLayers.RemoveAll(layer => screenLayersToRemove.Contains(layer));
            screenLayersToRemove.Clear();
        }
    }
}