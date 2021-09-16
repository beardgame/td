using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Selection
{
    abstract class SelectionListener :
        IListener<ObjectFocused>,
        IListener<ObjectFocusReset>,
        IListener<ObjectSelected>,
        IListener<ObjectSelectionReset>
    {
        public delegate void SelectionEventHandler();

        public void Subscribe(ComponentEvents events)
        {
            events.Subscribe<ObjectFocused>(this);
            events.Subscribe<ObjectFocusReset>(this);
            events.Subscribe<ObjectSelected>(this);
            events.Subscribe<ObjectSelectionReset>(this);
        }

        public void Unsubscribe(ComponentEvents events)
        {
            events.Unsubscribe<ObjectFocused>(this);
            events.Unsubscribe<ObjectFocusReset>(this);
            events.Unsubscribe<ObjectSelected>(this);
            events.Unsubscribe<ObjectSelectionReset>(this);
        }

        protected abstract void OnFocus();
        protected abstract void OnFocusReset();
        protected abstract void OnSelect();
        protected abstract void OnSelectionReset();

        public void HandleEvent(ObjectFocused @event) => OnFocus();
        public void HandleEvent(ObjectFocusReset @event) => OnFocusReset();
        public void HandleEvent(ObjectSelected @event) => OnSelect();
        public void HandleEvent(ObjectSelectionReset @event) => OnSelectionReset();

        public static SelectionListener Create(
            SelectionEventHandler? onFocus = null,
            SelectionEventHandler? onFocusReset = null,
            SelectionEventHandler? onSelect = null,
            SelectionEventHandler? onSelectionReset = null)
        {
            return new LambdaSelectionListener(onFocus, onFocusReset, onSelect, onSelectionReset);
        }

        private sealed class LambdaSelectionListener : SelectionListener
        {
            private readonly SelectionEventHandler? onFocus;
            private readonly SelectionEventHandler? onFocusReset;
            private readonly SelectionEventHandler? onSelect;
            private readonly SelectionEventHandler? onSelectionReset;

            public LambdaSelectionListener(
                SelectionEventHandler? onFocus,
                SelectionEventHandler? onFocusReset,
                SelectionEventHandler? onSelect,
                SelectionEventHandler? onSelectionReset)
            {
                this.onFocus = onFocus;
                this.onFocusReset = onFocusReset;
                this.onSelect = onSelect;
                this.onSelectionReset = onSelectionReset;
            }

            protected override void OnFocus() => onFocus?.Invoke();
            protected override void OnFocusReset() => onFocusReset?.Invoke();
            protected override void OnSelect() => onSelect?.Invoke();
            protected override void OnSelectionReset() => onSelectionReset?.Invoke();
        }
    }
}
