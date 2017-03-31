using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;

namespace Bearded.TD.Screens
{
    interface IScreenLayer
    {
        void Update(UpdateEventArgs args);
        // Should return false if the input should not be propagated.
        bool HandleInput(UpdateEventArgs args, InputState inputState);
        void OnResize(ViewportSize newSize);
        void Render(RenderContext context);
    }
}