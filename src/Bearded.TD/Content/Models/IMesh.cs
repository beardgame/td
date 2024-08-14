using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Loading;

namespace Bearded.TD.Content.Models;

interface IMesh
{
    DrawableMesh AsDrawable(
        IDrawableRenderers drawableRenderers,
        DrawOrderGroup drawGroup,
        int drawGroupOrderKey,
        Shader shader);
}
