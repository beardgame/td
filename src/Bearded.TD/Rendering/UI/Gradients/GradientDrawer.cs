using System;

namespace Bearded.TD.Rendering.UI.Gradients;

sealed class GradientDrawer(Gradients gradients)
{
    public GradientId AddGradient(Gradient gradient)
        => gradients.AddGradient(gradient.Stops);

    public GradientId AddGradient(ReadOnlySpan<GradientStop> stops)
        => gradients.AddGradient(stops);
}
