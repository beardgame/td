using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenTK.Input;

namespace Bearded.TD.UI.Controls
{
    sealed class FocusList : CompositeControl
    {
        private readonly IImmutableSet<Key> backwardKeys;
        private readonly IImmutableSet<Key> forwardKeys;
        private readonly bool isCyclic;

        private FocusList(IImmutableSet<Key> backwardKeys, IImmutableSet<Key> forwardKeys, bool isCyclic)
        {
            this.backwardKeys = backwardKeys;
            this.forwardKeys = forwardKeys;
            this.isCyclic = isCyclic;
            CanBeFocused = true;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

        public override void KeyReleased(KeyEventArgs eventArgs)
        {
            base.KeyReleased(eventArgs);

            if (eventArgs.Handled || !IsFocused) return;

            if (backwardKeys.Contains(eventArgs.Key))
            {
                var focusMoved = moveFocusBackward();
                if (focusMoved || isCyclic)
                    eventArgs.Handled = true;
            }
            else if (forwardKeys.Contains(eventArgs.Key))
            {
                var focusMoved = moveFocusForward();
                if (focusMoved || isCyclic)
                    eventArgs.Handled = true;
            }
        }

        private bool moveFocusBackward()
        {
            Console.WriteLine("backward");

            var desiredFocusIndex = focusedControlIndex()
                .Select(focusedIndex => focusedIndex - 1)
                .ValueOrDefault(Children.Count - 1);

            if (desiredFocusIndex < 0)
            {
                if (isCyclic)
                    desiredFocusIndex = Children.Count - 1;
                else
                    return false;
            }

            return Children[desiredFocusIndex].TryFocus();
        }

        private bool moveFocusForward()
        {
            Console.WriteLine("forward");

            var desiredFocusIndex = focusedControlIndex()
                .Select(focusedIndex => focusedIndex + 1)
                .ValueOrDefault(0);

            if (desiredFocusIndex >= Children.Count)
            {
                if (isCyclic)
                    desiredFocusIndex = 0;
                else
                    return false;
            }

            return Children[desiredFocusIndex].TryFocus();
        }

        private Maybe<int> focusedControlIndex() =>
            Children.Where(c => c.IsFocused).Select((c, i) => i).MaybeFirst();

        public class Builder
        {
            private readonly HashSet<Key> backwardKeys = new HashSet<Key>();
            private readonly HashSet<Key> forwardKeys = new HashSet<Key>();
            private bool isCyclic;

            public Builder AddBackwardKey(Key k)
            {
                backwardKeys.Add(k);
                return this;
            }

            public Builder AddForwardKey(Key k)
            {
                forwardKeys.Add(k);
                return this;
            }

            public Builder MakeCyclic()
            {
                isCyclic = true;
                return this;
            }

            public FocusList Build() => new FocusList(
                ImmutableHashSet.CreateRange(backwardKeys), ImmutableHashSet.CreateRange(forwardKeys), isCyclic);
        }
    }
}
