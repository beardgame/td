using System;
using System.Collections.Generic;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class RotatingListItemSource : IListItemSource
    {
        private readonly ListControl list;
        private readonly int capacity;
        private readonly double itemHeight;
        private readonly List<Control> controls;
        private int listStart;
        private int listEnd;

        public int ItemCount { get; private set; }

        public RotatingListItemSource(
            ListControl list, IEnumerable<Control> initialControls, int capacity, double itemHeight)
        {
            if (capacity < 2)
            {
                throw new ArgumentException("Capacity of a rotating list item source needs to be at least 2.");
            }

            this.list = list;
            this.capacity = capacity;
            this.itemHeight = itemHeight;
            controls = new List<Control>(capacity);
            controls.AddRange(initialControls);
            listEnd = controls.Count;
            ItemCount = controls.Count;
        }

        public void Push(Control control)
        {
            var pruned = false;
            if (ItemCount == capacity)
            {
                // Prune first half of list
                ItemCount /= 2;
                listStart = (listStart + (capacity - ItemCount)) % capacity;
                pruned = true;
            }

            if (listEnd < controls.Count)
            {
                controls[listEnd] = control;
            }
            else
            {
                controls.Add(control);
            }
            listEnd = (listEnd + 1) % capacity;
            ItemCount++;

            if (pruned)
            {
                list.Reload();
            }
            else
            {
                list.OnAppendItems(1);
            }
        }

        private int realIndex(int index) => (listStart + index) % capacity;

        public double HeightOfItemAt(int index) => itemHeight;

        public Control CreateItemControlFor(int index) => controls[realIndex(index)];

        public void DestroyItemControlAt(int index, Control control) { }
    }
}
