using System;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class DynamicLabel : Label
    {
        private readonly Func<string> stringProvider;

        public DynamicLabel(Func<string> stringProvider)
        {
            this.stringProvider = stringProvider;
        }

        protected override void RenderStronglyTyped(IRendererRouter r)
        {
            Text = stringProvider();
            r.Render(this);
        }
    }
}
