using System.Collections.Generic;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.Utilities;
using OpenTK.Input;

namespace Bearded.TD.UI.Controls
{
    class KeyBindings
    {
        public static IBinding OpenTechnology;
    }

    class ControlScheme
    {
        public Maybe<IBinding> BindingForEvent(InputEvent inputEvent)
        {
            return Maybe.Nothing;
        }
    }

    abstract class InputManagingControl : CompositeControl
    {
        private readonly ControlScheme controlScheme;
        private readonly Dictionary<IBinding, BindingState> states = new Dictionary<IBinding, BindingState>();

        protected InputManagingControl(ControlScheme controlScheme)
        {
            this.controlScheme = controlScheme;
        }

        public void AdvanceFrame()
        {
            foreach (var (_, entry) in states)
            {
                entry.AdvanceFrame();
            }
        }

        public override void KeyHit(KeyEventArgs eventArgs)
        {
            var e = InputEvent.ForKey(eventArgs.Key);
            controlScheme.BindingForEvent(e).Match(bindingTriggered);
        }

        public override void KeyReleased(KeyEventArgs eventArgs)
        {
            var e = InputEvent.ForKey(eventArgs.Key);
            controlScheme.BindingForEvent(e).Match(bindingReleased);
        }

        private void bindingTriggered(IBinding binding)
        {
            if (!states.ContainsKey(binding))
            {
                states[binding] = new BindingState();
            }
            states[binding].Trigger();
        }

        private void bindingReleased(IBinding binding)
        {
            states[binding].Release();
        }
    }

    struct InputEvent
    {
        public Maybe<Key> Key { get; }

        private InputEvent(Maybe<Key> key)
        {
            Key = key;
        }

        public static InputEvent ForKey(Key key) => new InputEvent(Maybe.Just(key));
    }

    class BindingState
    {
        public bool IsActive { get; private set; }
        public bool IsHit { get; private set; }
        public bool IsReleased { get; private set; }

        public void Trigger()
        {
            IsActive = true;
            IsHit = true;
        }

        public void Release()
        {
            IsActive = false;
            IsReleased = true;
        }

        public void AdvanceFrame()
        {
            IsHit = false;
            IsReleased = false;
        }
    }

    interface IBinding
    {
        IBindingInput Input { get; }
    }

    interface IBindingInput
    {
        bool Matches(InputEvent @event);
    }

    class KeyboardBindingInput : IBindingInput
    {
        private readonly Key key;

        public KeyboardBindingInput(Key key)
        {
            this.key = key;
        }

        public bool Matches(InputEvent @event) => @event.Key.Select(inputKey => inputKey == key).ValueOrDefault(false);
    }
}
