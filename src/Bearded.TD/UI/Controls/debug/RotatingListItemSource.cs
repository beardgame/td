using System;
using System.Collections.Generic;
using Bearded.UI.Controls;

sealed class RotatingListItemSource : IListItemSource
{
    private readonly ListControl list;
    private readonly int capacity;
    private readonly List<Control> controls;
    private int listStart;
    private int listEnd;

    public int ItemCount { get; private set; }

    public RotatingListItemSource(ListControl list, int capacity)
    {
        if (capacity < 2)
        {
            throw new ArgumentException("Capacity of a rotating list item source needs to be at least 2.");
        }

        this.list = list;
        this.capacity = capacity;
        controls = new List<Control>(capacity);
    }

    public void Push(Control control)
    {
        if (ItemCount == capacity)
        {
            // Prune first half of list
            ItemCount /= 2;
            listStart = (listStart + (capacity - ItemCount)) % capacity;
        }

        controls[listEnd] = control;
        listEnd = (listEnd + 1) % capacity;
        ItemCount++;

        list.Reload();
    }

    private int realIndex(int index) => (listStart + index) % capacity;

    public double HeightOfItemAt(int index) => controls[realIndex(index)].Frame.Size.Y;

    public Control CreateItemControlFor(int index) => controls[realIndex(index)];

    public void DestroyItemControlAt(int index, Control control) { }
}
