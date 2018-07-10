using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;
using Bearded.Utilities;

namespace Bearded.UI.Controls
{
    // TODO: extract scroll controls
    // TODO: make scoll bar
    // TODO: allow insert/removal/update of ranges

    public interface IListItemSource
    {
        int ItemCount { get; }
        double HeightOfItemAt(int index);

        Control CreateItemControlFor(int index);
        void DestroyItemControlAt(int index, Control control);
    }

    public class ListControl : CompositeControl
    {
        private readonly IListItemSource itemSource;
        private readonly CompositeControl listContainer;
        private readonly CompositeControl contentContainer;

        private int firstCellIndex;
        private readonly LinkedList<(Control Control, double Offset, double Height)> cells = new LinkedList<(Control, double, double)>();
        private double totalContentHeight;

        public bool StickToBottom { get; set; } = true;
        private bool currentlyStuckToBottom;

        private double contentTopLimit;
        private double contentBottomLimit;

        public double ScrollOffset
        {
            get => contentTopLimit;
            private set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (contentTopLimit == value)
                    return;

                contentTopLimit = value;

                validateScrollPosition();
                updateContentContainerAnchors();
            }
        }

        public ListControl(IListItemSource itemSource, CompositeControl listContainer = null)
        {
            this.itemSource = itemSource;
            this.listContainer = listContainer ?? new CompositeControl();

            Add(this.listContainer);

            contentContainer = new CompositeControl();
            this.listContainer.Add(contentContainer);

            calculateTotalHeight();
        }

        public override void MouseScrolled(MouseScrollEventArgs eventArgs)
        {
            var delta = eventArgs.DeltaScrollF * 30;
            var offsetBefore = ScrollOffset;

            ScrollOffset = offsetBefore + delta;

            var offsetAfter = ScrollOffset;

            if (offsetAfter > offsetBefore)
            {
                onScrollDown();
            }
            else if (offsetAfter < offsetBefore)
            {
                onScrollUp();
            }
        }

        public void ScrollToTop()
        {
            ScrollOffset = 0;
            onScrollUp();
        }

        public void ScrollToBottom()
        {
            ScrollOffset = totalContentHeight;
            onScrollDown();
        }

        private void onScrollUp()
        {
            addCellsUpwards();
            removeCellsUpwards();

            //! handle case where scrolling with no overlap
            // can probably just do this in the 'addCells' method by skipping smartly if new cell would be invisible
        }

        private void onScrollDown()
        {
            addCellsDownwards();
            removeCellsDownwards();

            //! handle case where scrolling with no overlap
            // can probably just do this in the 'addCells' method by skipping smartly if new cell would be invisible
        }

        private void onRemoveHead(int i)
        {
            // get height of removed items
            // offset scroll accordingly
            // validate scroll position

            // add cells downwards (order?)
            // remove cells downwards
        }

        private void onAddTail(int i)
        {
            // update total height
            // if stuck to bottom, scroll
            // add cells downwards
            // remove cells downwards
        }

        protected override void FrameChanged()
        {
            base.FrameChanged();

            validateScrollPosition();
            addCellsDownwards();
            removeCellsUpwards();
            addCellsUpwards();
        }
        
        public void Reload()
        {
            if (contentContainer.Children.Count > 0)
                clearChildren();

            calculateTotalHeight();

            if (currentlyStuckToBottom)
                ScrollOffset = totalContentHeight;

            validateScrollPosition();
            addCellsDownwards();
        }

        private void calculateTotalHeight()
        {
            totalContentHeight = Enumerable
                .Range(0, itemSource.ItemCount)
                .Sum(i => itemSource.HeightOfItemAt(i));
        }

        private void updateContentContainerAnchors()
        {
            contentContainer.SetAnchors(
                Anchors.Default.H,
                new Anchors(new Anchor(0, -ScrollOffset), new Anchor(0, totalContentHeight - ScrollOffset)).V
                );
        }

        private void validateScrollPosition()
        {
            const double minimum = 0;
            var maximum = getMaximumScrollOffset();

            ScrollOffset = ScrollOffset.Clamped(minimum, maximum);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            currentlyStuckToBottom = StickToBottom && ScrollOffset == maximum;

            updateContentLimits();
        }

        private double getMaximumScrollOffset()
            => Math.Max(0, totalContentHeight - listContainer.Frame.Size.Y);

        private void updateContentLimits()
        {
            contentBottomLimit = contentTopLimit + listContainer.Frame.Size.Y;
        }

        private void removeCellsUpwards()
        {
            var lastIndexInList = firstCellIndex + cells.Count;

            while (cells.Count > 0)
            {
                var lastCell = cells.Last.Value;
                
                if (lastCell.Offset < contentBottomLimit)
                    break;
                
                itemSource.DestroyItemControlAt(lastIndexInList, lastCell.Control);

                contentContainer.Remove(lastCell.Control);
                cells.RemoveLast();

                lastIndexInList--;
            }
        }

        private void removeCellsDownwards()
        {
            while (cells.Count > 0)
            {
                var firstCell = cells.First.Value;

                if (bottomOf(firstCell) > contentTopLimit)
                    break;

                itemSource.DestroyItemControlAt(firstCellIndex, firstCell.Control);

                contentContainer.Remove(firstCell.Control);
                cells.RemoveFirst();

                firstCellIndex++;
            }
        }

        private void addCellsUpwards()
        {
            var firstCellTop = cells.Count == 0 ? 0 : cells.First.Value.Offset;

            while (firstCellIndex > 0)
            {
                if (firstCellTop < contentTopLimit)
                    break;

                firstCellIndex--;

                var cell = addCellAbove(firstCellIndex, firstCellTop);

                firstCellTop = cell.Offset;
            }
        }

        private void addCellsDownwards()
        {
            var nextIndexAfterLastCell = firstCellIndex + cells.Count;

            var previousCellBottom = cells.Count == 0
                ? 0
                : bottomOf(cells.Last.Value);
            
            while (nextIndexAfterLastCell < itemSource.ItemCount)
            {
                if (previousCellBottom > contentBottomLimit)
                    break;

                var cell = addCellBelow(nextIndexAfterLastCell, previousCellBottom);

                previousCellBottom = bottomOf(cell);
                nextIndexAfterLastCell++;
            }
        }

        private (Control Control, double Offset, double Height)
            addCellBelow(int index, double top)
        {
            var height = itemSource.HeightOfItemAt(index);
            var bottom = top + height;

            var cell = createCell(index, bottom, top, height);

            cells.AddLast(cell);

            return cell;
        }

        private (Control Control, double Offset, double Height)
            addCellAbove(int index, double bottom)
        {
            var height = itemSource.HeightOfItemAt(index);
            var top = bottom - height;

            var cell = createCell(index, bottom, top, height);

            cells.AddFirst(cell);

            return cell;
        }

        private (Control Control, double Offset, double Height)
            createCell(int index, double bottom, double top, double height)
        {
            var control = itemSource.CreateItemControlFor(index);

            anchorCell(control, top, bottom);
            contentContainer.Add(control);

            var cell = (control, top, height);


            return cell;
        }

        private static void anchorCell(Control control, double cellTop, double cellBottom)
        {
            control.SetAnchors(
                Anchors.Default.H,
                new Anchors(new Anchor(0, cellTop), new Anchor(0, cellBottom)).V
            );
        }

        private double bottomOf((Control Control, double Offset, double Height) cell)
        {
            return cell.Offset + cell.Height;
        }

        private void clearChildren()
        {
            foreach (var (control, index) in cells.Select((cell, i) => (cell.Control, firstCellIndex + i)))
            {
                itemSource.DestroyItemControlAt(index, control);
            }
            contentContainer.RemoveAllChildren();
            cells.Clear();
            firstCellIndex = 0;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
