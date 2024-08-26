using System;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;

namespace Bearded.TD.UI.Controls;

[Obsolete("Use the base class again, after https://github.com/beardgame/ui/pull/127 is merged and we use it.")]
class TDButton : Button
{
    public override void MouseButtonHit(MouseButtonEventArgs eventArgs)
    {
        base.MouseButtonHit(eventArgs);
        eventArgs.Handled = true;
    }
}
