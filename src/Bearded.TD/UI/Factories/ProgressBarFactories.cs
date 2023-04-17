using Bearded.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using static Bearded.TD.Constants.UI.ProgressBar;

namespace Bearded.TD.UI.Factories;

static class ProgressBarFactories
{
    public static Control BareProgressBar(Binding<double> progress, Color? color = null)
    {
        var barControl = new BackgroundBox(color ?? DefaultColor)
            .Anchor(a => a.Right(relativePercentage: progress.Value));
        progress.SourceUpdated += p => barControl.Anchor(a => a.Right(relativePercentage: p));
        return barControl;
    }

    public static Control ProgressBar(Binding<double> progress, Color? color = null)
    {
        return new CompositeControl
        {
            BareProgressBar(progress, color),
            new Border()
        };
    }

    public static Layouts.IColumnLayout AddProgressBar(
        this Layouts.IColumnLayout columnLayout, Binding<double> progress, Color? color = null)
    {
        return columnLayout.Add(ProgressBar(progress, color), Height + 2 * Margin);
    }
}
