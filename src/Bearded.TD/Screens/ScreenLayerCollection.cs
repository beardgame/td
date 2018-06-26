using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities.Input;

namespace Bearded.TD.Screens
{
    class ScreenLayerCollection
    {
        private readonly List<IScreenLayer> screenLayers = new List<IScreenLayer>();
        private readonly HashSet<IScreenLayer> screenLayersToRemove = new HashSet<IScreenLayer>();
        private readonly List<Action> delayedScreenLayerActions = new List<Action>();
        private ViewportSize viewportSize;

        private bool isEnumerating;

        protected void PropagateInput(UpdateEventArgs args, InputState inputState)
        {
            isEnumerating = true;
            for (var i = screenLayers.Count - 1; i >= 0; i--)
                screenLayers[i].HandleInput(args, inputState);
            isEnumerating = false;
        }
        
        protected void UpdateAll(UpdateEventArgs args)
        {
            isEnumerating = true;
            screenLayers.ForEach(layer => layer.Update(args));
            isEnumerating = false;
            doDelayedScreenLayerActions();
            removeScreenLayersQueuedForRemoval();
        }

        public void OnResize(ViewportSize newSize)
        {
            viewportSize = newSize;
            screenLayers.ForEach((layer) => layer.OnResize(newSize));
        }

        public void Render(RenderContext context)
        {
            isEnumerating = true;
            screenLayers.ForEach((layer) => layer.Render(context));
            isEnumerating = false;
        }

        public void AddScreenLayerOnTop(IScreenLayer screenLayer)
        {
            if (isEnumerating && queueIfEnumerating(() => AddScreenLayerOnTop(screenLayer)))
                return;
            screenLayers.Add(screenLayer);
            initializeScreenLayer(screenLayer);
        }

        public void AddScreenLayerOnTopOf(IScreenLayer reference, IScreenLayer toAdd)
        {
            if (isEnumerating && queueIfEnumerating(() => AddScreenLayerOnTopOf(reference, toAdd)))
                return;
            // Equivalent to adding screen layer _after_ reference.
            var indexOfRef = screenLayers.IndexOf(reference);
            if (indexOfRef == screenLayers.Count)
                AddScreenLayerOnTop(toAdd);
            else
                addScreenLayerAtIndex(indexOfRef + 1, toAdd);
        }

        public void AddScreenLayerBehind(IScreenLayer reference, IScreenLayer toAdd)
        {
            if (isEnumerating && queueIfEnumerating(() => AddScreenLayerBehind(reference, toAdd)))
                return;
            // Equivalent to adding screen layer _before_ reference.
            addScreenLayerAtIndex(screenLayers.IndexOf(reference), toAdd);
        }

        private void addScreenLayerAtIndex(int index, IScreenLayer screenLayer)
        {
            screenLayers.Insert(index, screenLayer);
            initializeScreenLayer(screenLayer);
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

        private bool queueIfEnumerating(Action action)
        {
            if (!isEnumerating)
                return false;


            delayedScreenLayerActions.Add(action);
            return true;
        }

        private void doDelayedScreenLayerActions()
        {
            if (delayedScreenLayerActions.Count == 0)
                return;
            delayedScreenLayerActions.ForEach(action => action());
            delayedScreenLayerActions.Clear();
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