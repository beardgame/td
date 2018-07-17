using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class RotatingListItemSource<T> : IListItemSource
    {
        private readonly ListControl list;
        private readonly Func<T, Control> controlFactory;
        private readonly double itemHeight;
        private readonly T[] items;
        private int listStart;
        private int listEnd;

        public int ItemCount { get; private set; }

        public RotatingListItemSource(ListControl list,
            IEnumerable<T> initialControls,
            Func<T, Control> controlFactory,
            double itemHeight,
            int capacity)
        {
            if (capacity < 2)
            {
                throw new ArgumentException("Capacity of a rotating list item source needs to be at least 2.");
            }

            this.list = list;
            this.controlFactory = controlFactory;
            this.itemHeight = itemHeight;
            items = new T[capacity];

            var initialControlsList = initialControls.ToList();
            initialControlsList.CopyTo(items, 0);
            listEnd = initialControlsList.Count;
            ItemCount = initialControlsList.Count;
        }

        public void Push(T item)
        {
            var pruned = false;
            if (ItemCount == items.Length)
            {
                // Prune first half of list
                ItemCount /= 2;
                listStart = (listStart + (items.Length - ItemCount)) % items.Length;
                pruned = true;
            }

            items[listEnd] = item;
            listEnd = (listEnd + 1) % items.Length;
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

        private int realIndex(int index) => (listStart + index) % items.Length;

        public double HeightOfItemAt(int index) => itemHeight;

        public Control CreateItemControlFor(int index) => controlFactory(items[realIndex(index)]);

        public void DestroyItemControlAt(int index, Control control) { }
    }
}
