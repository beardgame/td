﻿using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Controls;

sealed class VersionOverlayControl : CompositeControl
{
    public VersionOverlayControl(VersionOverlay versionOverlay)
    {
        IsClickThrough = true;
        Add(new Label
        {
            Text = versionOverlay.VersionCodeString,
            FontSize = 14,
            TextAnchor = Vector2d.One,
            IsClickThrough = true
        });
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}
