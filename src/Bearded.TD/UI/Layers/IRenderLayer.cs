﻿using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.UI.Layers
{
    interface IRenderLayer
    {
        Matrix4 ViewMatrix { get; }
        Matrix4 ProjectionMatrix { get; }
        RenderOptions RenderOptions { get; }
        void Draw();
    }

    interface IDeferredRenderLayer : IRenderLayer
    {
        float FarPlaneDistance { get; }
        float Time { get; }
        ContentSurfaceManager DeferredSurfaces { get; }
    }
}
