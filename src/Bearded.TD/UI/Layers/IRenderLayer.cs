﻿using Bearded.TD.Rendering;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Layers;

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
    float HexagonalFallOffDistance { get; }
    ContentRenderers ContentRenderers { get; }
}
