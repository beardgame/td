using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;

namespace Bearded.TD.Screens
{
    interface IScreenLayer
    {
        void Update(UpdateEventArgs args);
        bool HandleInput(UpdateEventArgs args, InputState inputState);
        void OnResize(ViewportSize newSize);
        void Render(RenderContext context);
    }
}