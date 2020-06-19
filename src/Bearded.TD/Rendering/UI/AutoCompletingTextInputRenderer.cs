using amulware.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering.UI
{
    sealed class AutoCompletingTextInputRenderer : IRenderer<AutoCompletingTextInput>
    {
        private readonly TextInputRenderer internalRenderer;
        private readonly FontGeometry geometry;

        public AutoCompletingTextInputRenderer(IndexedSurface<PrimitiveVertexData> primitives, IndexedSurface<UVColorVertexData> fontSurface, Font font)
        {
            internalRenderer = new TextInputRenderer(primitives, fontSurface, font);

            geometry = new FontGeometry(fontSurface, font)
            {
                Color = Color.White * .5f,
            };
        }

        public void Render(AutoCompletingTextInput textInput)
        {
            internalRenderer.Render(textInput);

            geometry.Height = (float)textInput.FontSize;

            var textWidth = geometry.StringWidth(textInput.Text);
            geometry.DrawString(
                (Vector2) textInput.Frame.TopLeft + textWidth * Vector2.UnitX,
                textInput.AutoCompletionText.Substring(textInput.Text.Length));
        }
    }
}
