using System;

namespace Bearded.TD.Game.Simulation.Events
{
    static class Listener
    {
        public static IListener<T> ForEvent<T>(Action<T> onEvent) where T : IEvent => new LambdaListener<T>(onEvent);

        private sealed class LambdaListener<T> : IListener<T> where T : IEvent
        {
            private readonly Action<T> onEvent;

            public LambdaListener(Action<T> onEvent)
            {
                this.onEvent = onEvent;
            }

            public void HandleEvent(T @event) => onEvent(@event);
        }
    }
}
