using System;
using amulware.Graphics;

namespace Bearded.TD.UI.Controls
{
    sealed class DynamicDot : Dot
    {
        private readonly Func<Color> colorProvider;

        public override Color Color => colorProvider();

        public DynamicDot(Func<Color> colorProvider)
        {
            this.colorProvider = colorProvider;
        }
    }
}
