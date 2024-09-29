using System;

namespace Bearded.TD.Rendering.Deferred.Level;

interface IHeightmapToLevelRenderer : IDisposable
{
    void RenderAll();
    void Resize(float scale);
}
