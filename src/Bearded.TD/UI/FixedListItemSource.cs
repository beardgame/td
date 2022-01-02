using System.Collections.Generic;
using Bearded.UI.Controls;

namespace Bearded.TD.UI;

sealed class FixedListItemSource : IListItemSource
{
    private readonly IList<Control> controls;
    private readonly double controlHeight;

    public int ItemCount => controls.Count;

    public FixedListItemSource(IList<Control> controls, double controlHeight)
    {
        this.controls = controls;
        this.controlHeight = controlHeight;
    }

    public double HeightOfItemAt(int index) => controlHeight;

    public Control CreateItemControlFor(int index) => controls[index];

    public void DestroyItemControlAt(int index, Control control) {}
}