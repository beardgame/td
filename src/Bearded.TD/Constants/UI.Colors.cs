using System.Collections.Generic;
using Bearded.Graphics;

namespace Bearded.TD;

public enum BackgroundColor
{
    Default = 0,
    InactiveElement = 1,
    Tooltip = 1,
    ActiveElement = 2,
    SubtleOutline = 2,
    Element = 4,
    Hover = 5,
}

public enum ForeGroundColor
{
    Highlight = 0,
    Text = 1,
    Headline = 2,
    DisabledText = 6,
}

static partial class Constants
{
    public static partial class UI
    {
        public static class Colors
        {
            private static readonly IReadOnlyList<Color> purples =
            [
                new Color(0xFFf5e8e3),
                new Color(0xFFe3ceca),
                new Color(0xFFd0b3b3),
                new Color(0xFFbe9ca0),
                new Color(0xFFab8790),
                new Color(0xFF987481),
                new Color(0xFF866173),
                new Color(0xFF745066),
                new Color(0xFF614059),
                new Color(0xFF4e314b),
                new Color(0xFF3c233b),
                new Color(0xFF27172a),
                new Color(0xFF140c17),
            ];

            private static readonly IReadOnlyList<Color> yellows =
            [
                new Color(0xFFf5e8e3),
                new Color(0xFFe3cfc8),
                new Color(0xFFd1b8ae),
                new Color(0xFFbfa296),
                new Color(0xFFad8d80),
                new Color(0xFF9b796c),
                new Color(0xFF8a6659),
                new Color(0xFF785548),
                new Color(0xFF664438),
                new Color(0xFF54352a),
                new Color(0xFF42281e),
                new Color(0xFF301b14),
                new Color(0xFF1e100b),
            ];

            public static Color Get(BackgroundColor level)
                => purples[^((int)level + 1)];
            public static Color Get(ForeGroundColor level)
                => yellows[(int)level];
        }
    }
}
