using System.Collections.Generic;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    class TextInput : Bearded.UI.Controls.TextInput
    {
        private static readonly HashSet<char> allowedChars = new HashSet<char> { ' ', '-', '_', '.', '+', '"', ':' };

        public double FontSize { get; set; } = 24;

        public override void CharacterTyped(CharEventArgs eventArgs)
        {
            if (!characterIsValid(eventArgs.Character))
                return;

            base.CharacterTyped(eventArgs);
        }

        private static bool characterIsValid(char c)
        {
            return (c >= '0' && c <= '9')
                || (c >= '@' && c <= 'Z')
                || (c >= 'a' && c <= 'z')
                || allowedChars.Contains(c);
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
