using System;
using Bearded.Graphics;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class DynamicDot(IReadonlyBinding<Color> color) : Dot
{
    public override Color Color => color.Value;
}
