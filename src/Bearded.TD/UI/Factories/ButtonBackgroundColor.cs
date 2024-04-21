using Bearded.Graphics;

namespace Bearded.TD.UI.Factories;

readonly record struct ButtonBackgroundColor(
    Color? Neutral = null,
    Color? Hover = null,
    Color? Active = null,
    Color? Disabled = null
)
{
    public static ButtonBackgroundColor operator *(ButtonBackgroundColor colors, float scalar)
    {
        return new ButtonBackgroundColor(
            colors.Neutral * scalar,
            colors.Hover * scalar,
            colors.Active * scalar,
            colors.Disabled * scalar
        );
    }
};
