using System;
using Bearded.Graphics;

namespace Bearded.TD.UI.Controls
{
    sealed class DynamicBorder : Border
    {
        private readonly Func<Color> colorProvider;

        public override Color Color => colorProvider();

        public DynamicBorder(Func<Color> colorProvider)
        {
            this.colorProvider = colorProvider;
        }
    }
}
