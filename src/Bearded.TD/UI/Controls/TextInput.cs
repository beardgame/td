using System.Collections.Generic;
using Bearded.TD.UI.Factories;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

class TextInput : Bearded.UI.Controls.TextInput
{
    private static readonly HashSet<char> allowedChars = new HashSet<char> { ' ', '-', '_', '.', '+', '"', ':', '@' };

    public double FontSize { get; set; } = 24;

    public bool AllowDigits { get; set; } = true;
    public bool AllowLetters { get; set; } = true;
    public bool AllowSpecialCharacters { get; set; } = true;

    public MouseStateObserver MouseState { get; }

    public TextInput()
    {
        MouseState = new MouseStateObserver(this);
    }

    public override void CharacterTyped(CharEventArgs eventArgs)
    {
        if (!characterIsValid(eventArgs.Character))
            return;

        base.CharacterTyped(eventArgs);
    }

    private bool characterIsValid(char c)
    {
        return (AllowDigits && c >= '0' && c <= '9')
            || (AllowLetters && c >= 'A' && c <= 'Z')
            || (AllowLetters && c >= 'a' && c <= 'z')
            || (AllowSpecialCharacters && allowedChars.Contains(c));
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}
