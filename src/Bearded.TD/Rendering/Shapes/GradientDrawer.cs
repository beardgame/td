using System;
using Bearded.TD.UI.Shapes;

namespace Bearded.TD.Rendering.Shapes;

sealed class GradientDrawer(Gradients gradients)
{
    public GradientId AddGradient(Gradient gradient)
        => gradients.AddGradient(gradient.Stops);

    public GradientId AddGradient(ReadOnlySpan<GradientStop> stops)
        => gradients.AddGradient(stops);
}
