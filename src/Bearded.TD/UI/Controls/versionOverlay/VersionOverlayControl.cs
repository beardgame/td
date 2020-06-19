using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenToolkit.Mathematics;

namespace Bearded.TD.UI.Controls
{
    sealed class VersionOverlayControl : CompositeControl
    {
        public VersionOverlayControl(VersionOverlay versionOverlay)
        {
            Add(new Label
            {
                Text = versionOverlay.VersionCodeString,
                FontSize = 14,
                TextAnchor = Vector2d.One,
            });
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
