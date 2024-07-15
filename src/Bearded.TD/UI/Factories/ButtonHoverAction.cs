using Bearded.TD.Game.Overlays;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories;

static class ButtonHoverAction
{
    public delegate T HoverStartEffect<out T>();

    public delegate void HoverEndEffect<in T>(T state);

    public static void AddHoverAction<T>(
        this Button button, HoverStartEffect<T> startHover, HoverEndEffect<T> endHover)
    {
        var mouseStateObserver = new MouseStateObserver(button);
        var effect = new HoverEffect<T>(mouseStateObserver, startHover, endHover);
        mouseStateObserver.StateChanged += effect.OnStateChanged;
    }

    private sealed class HoverEffect<T>(
        MouseStateObserver observer, HoverStartEffect<T> startHover, HoverEndEffect<T> endHover)
    {
        private bool hoverState;
        private T state = default!;

        public void OnStateChanged()
        {
            if (observer.MouseIsOver == hoverState) return;
            if (observer.MouseIsOver)
            {
                onHoverStart();
            }
            else
            {
                onHoverEnd();
            }
        }

        private void onHoverStart()
        {
            state = startHover();
            hoverState = true;
        }

        private void onHoverEnd()
        {
            endHover(state);
            hoverState = false;
            state = default!;
        }
    }
}
