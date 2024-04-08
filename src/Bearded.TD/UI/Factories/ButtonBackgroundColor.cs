using Bearded.Graphics;

namespace Bearded.TD.UI.Factories;

public sealed record ButtonBackgroundColor(
    Color? Neutral = null,
    Color? Hover = null,
    Color? Active = null,
    Color? Disabled = null
);
