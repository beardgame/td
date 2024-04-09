using Bearded.Graphics;

namespace Bearded.TD.UI.Factories;

readonly record struct ButtonBackgroundColor(
    Color? Neutral = null,
    Color? Hover = null,
    Color? Active = null,
    Color? Disabled = null
);
