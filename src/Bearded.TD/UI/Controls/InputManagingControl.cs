using System.Collections.Generic;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.Utilities;
using OpenTK.Input;

namespace Bearded.TD.UI.Controls
{
    class InputAction
    {

    }

    abstract class InputManagingControl : CompositeControl
    {
        private IEnumerable<(IBindingInput, BindingInputState)> states;

        protected InputManagingControl()
        {

        }

        public override void KeyHit(KeyEventArgs eventArgs)
        {
            var e = InputEvent.ForKey(eventArgs.Key);
            states.MaybeFirst(tuple => tuple.Item1.Matches(e)).Match(tuple => tuple.Item2.Trigger());
        }

        public override void KeyReleased(KeyEventArgs eventArgs)
        {
            // Don't consider modifier keys for release. The modifier keys may have been released already.
            var e = InputEvent.ForKey(eventArgs.Key);
            states.MaybeFirst(tuple => tuple.Item1.Matches(e)).Match(tuple => tuple.Item2.Release());
        }

        public static InputManagingControl ForBindings(IEnumerable<IBinding> bindings)
        {
            return null;
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

    class BindingInputState
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
