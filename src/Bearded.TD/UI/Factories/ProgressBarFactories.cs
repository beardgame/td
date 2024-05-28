using System;
using Bearded.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using static Bearded.TD.Constants.UI.ProgressBar;

namespace Bearded.TD.UI.Factories;

static class ProgressBarFactories
{
    [Obsolete]
    public static Control BareProgressBar(IReadonlyBinding<double> progress, Color? color = null)
    {
        var barControl = new BackgroundBox(color ?? DefaultColor)
            .Anchor(a => a.Right(relativePercentage: progress.Value));
        progress.SourceUpdated += p => barControl.Anchor(a => a.Right(relativePercentage: p));
        return barControl;
    }
}
