using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.UI.Controls;

sealed partial class BuildingStatusControl
{
    private sealed class IconRow<T> : CompositeControl, ICollectionChangeHandler<IReadonlyBinding<T>>
    {
        private readonly ReadOnlyObservableCollection<IReadonlyBinding<T>> source;
        private readonly Func<IReadonlyBinding<T>, Control> controlFactory;
        private readonly List<Control> iconControls = [];
        private IDisposable? listener;

        public IconRow(
            ReadOnlyObservableCollection<IReadonlyBinding<T>> source,
            Func<IReadonlyBinding<T>, Control> controlFactory,
            ShapeComponents? background = null)
        {
            this.source = source;
            this.controlFactory = controlFactory;

            if (background is { } components)
            {
                Add(new ComplexBox { Components = components }
                    .Anchor(a => a.Left(margin: Constants.UI.BuildingStatus.StatusRowBackgroundLeftMargin)));
            }
        }

        protected override void OnAddingToParent()
        {
            State.Satisfies(listener is null);
            createInitialControls();
            listener = source.ConsumeChanges(this);
            base.OnAddingToParent();
        }

        protected override void OnRemovingFromParent()
        {
            State.Satisfies(listener is not null);
            listener?.Dispose();
            OnReset();
            base.OnRemovingFromParent();
        }

        private void createInitialControls()
        {
            foreach (var item in source)
            {
                var control = controlFactory(item);
                Add(control);
                iconControls.Add(control);
            }

            positionControls(Range.All);
        }

        public void OnItemAdded(IReadonlyBinding<T> item, int index)
        {
            var control = controlFactory(item);
            Add(control);

            // Insertions at the end are legal, and just operate like an `Add`.
            iconControls.Insert(index, control);
            // We want to ensure that the newly added control is positioned correctly. Any controls to the right of it
            // also need to be repositioned.
            positionControls(index..);
        }

        public void OnItemRemoved(IReadonlyBinding<T> item, int index)
        {
            var control = iconControls[index];
            Remove(control);

            iconControls.RemoveAt(index);
            // After removing the control, any controls to the right of it need to collapse back to the left, starting
            // with the control now occupying the index-th slot.
            positionControls(index..);
        }

        public void OnItemReplaced(IReadonlyBinding<T> oldItem, IReadonlyBinding<T> newItem, int index)
        {
            // Not the most performant perhaps, but happens rarely enough that ease of implementation is worth it.
            OnItemRemoved(oldItem, index);
            OnItemAdded(newItem, index);
        }

        public void OnItemMoved(IReadonlyBinding<T> item, int oldIndex, int newIndex)
        {
            // Not the most performant perhaps, but happens rarely enough that ease of implementation is worth it.
            // Perhaps in the future this is something we may want to implement if we want to animate this, but for now
            // this is not actually behaviour we expect to happen in normal circumstances.
            OnItemRemoved(item, oldIndex);
            OnItemAdded(item, newIndex);
        }

        public void OnReset()
        {
            // We cannot use `RemoveAllChildren` here because there may be background components.
            foreach (var control in iconControls)
            {
                Remove(control);
            }
            iconControls.Clear();
        }

        private void positionControls(Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(iconControls.Count);
            for (var i = offset; i < offset + length; i++)
            {
                positionControl(i);
            }
        }

        private void positionControl(int index)
        {
            var control = iconControls[index];
            control.Anchor(a => a
                .Left(margin: buttonLeftMargin(index), width: Constants.UI.BuildingStatus.ButtonSize)
                .Top(relativePercentage: 0.5, margin: -0.5 * Constants.UI.BuildingStatus.ButtonSize, height: Constants.UI.BuildingStatus.ButtonSize)
            );
        }
    }
}
