using Bearded.Graphics;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD;

static partial class Constants
{
    public static partial class UI
    {
        public const double LayoutMarginSmall = 4;
        public const double LayoutMargin = 8;

        public static class Shadows
        {
            public static readonly Shadow Default = Shadow((0, 2, 0), 5, new Color(0, 3, 13, 255) * 0.5f);
            public static readonly Shadow LargeWindow = Shadow((0, 10, 0), 50, new Color(0, 3, 13, 255) * 0.5f);
        }

        public static class Button
        {
            public const float FontSize = 18;
            public const float CostFontSize = 14;

            public const double Height = 32;
            public const double Width = 160;
            public const double Margin = 4;

            public const double SquareButtonSize = 64;

            public static readonly Color ActiveColor = Colors.Get(BackgroundColor.ActiveElement);
            public static readonly Color HoverColor = Colors.Get(BackgroundColor.Hover);

            public static readonly Shadow DefaultShadow = Shadows.Default;
        }

        public static class Checkbox
        {
            public const double Size = Button.Height;
        }

        public static class Console
        {
            public const double LogEntryHeight = 20;
            public const double InputHeight = 24;
            public const double FontSize = 16;
            public const TextStyle Font = TextStyle.Monospace;
        }

        public static class Form
        {
            public const double DenseFormRowHeight = 36;
            public const double FormRowHeight = 48;

            public const double InputHeight = Button.Height;
            public const double InputWidth = Button.Width;
        }

        public static class Menu
        {
            public const float ShadowWidth = 15;
            public static readonly Color ShadowColor = Shadows.Default.Color * 0.5f;
            public const double Width = 300;
        }

        public static class NavBar
        {
            public const double Height = 48;
        }

        public static class ProgressBar
        {
            public static readonly Color DefaultColor = Color.PaleGreen;
            public const double Height = 8;
            public const double Margin = 4;
        }

        public static class Statistics
        {
            public static readonly TimeSpan TimeBetweenUIUpdates = 0.25.S();
        }

        public static class TabBar
        {
            public const double Height = NavBar.Height;
        }

        public static class Text
        {
            public const float HeaderFontSize = 24;
            public const float HeaderLineHeight = 32;

            public const float SubHeaderFontSize = 20;
            public const float SubHeaderLineHeight = 24;

            public const float FontSize = 16;
            public const float LineHeight = 20;

            public static readonly Color TextColor = Colors.Get(ForeGroundColor.Text);
            public static readonly Color ErrorTextColor = Color.Red;
            public static readonly Color DisabledTextColor = Colors.Get(ForeGroundColor.DisabledText);
        }

        public static class Tooltip
        {
            public const double DefaultWidth = 300;
            public const double Margin = 4;
            public const double AnchorMargin = 4;
        }

        public static class Window
        {
            public const double TitlebarHeight = NavBar.Height;
        }
    }
}
