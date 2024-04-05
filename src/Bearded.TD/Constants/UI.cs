using System;
using Bearded.Graphics;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using static Bearded.TD.Rendering.Shapes.Shapes;
using static Bearded.Utilities.Interpolate;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD;

static partial class Constants
{
    public static partial class UI
    {
        public const double LayoutMarginSmall = 4;
        public const double LayoutMargin = 8;

        public static class Layout
        {
            public const double HorizontalSeparatorHeight = 1;

            public static readonly ShapeComponents HorizontalSeparator =
            [
                Fill.With(ShapeColor.From(
                    [
                        (0, Color.Transparent),
                        (0.25, Colors.Get(BackgroundColor.WindowInsetLine)),
                        (0.75, Colors.Get(BackgroundColor.WindowInsetLine)),
                        (1, Color.Transparent),
                    ],
                    GradientDefinition.Linear(AnchorPoint.Relative((0, 0)), AnchorPoint.Relative((1, 0))))
                ),
            ];
        }

        public static class Shadows
        {
            public static readonly Color DefaultColor = new(0, 3, 13);

            public static readonly Shadow Default = Shadow((0, 2, 0), 5, DefaultColor * 0.5f);
            public static readonly Shadow SmallWindow = Shadow((0, 6, 0), 50, DefaultColor * 0.65f);
            public static readonly Shadow LargeWindow = Shadow((0, 10, 0), 75, DefaultColor * 0.75f);
        }

        public static class Button
        {
            public const float FontSize = 18;
            public const float CostFontSize = 14;

            public const double Height = 32;
            public const double Width = 160;
            public const double Margin = 4;

            public const double SquareButtonSize = 64;
            public const double SmallSquareButtonSize = 32;

            public static readonly Shadow DefaultShadow = Shadows.Default;

            public static AnimationBlueprint<(Action<Color> set, Color from, Color to)> BackgroundColorAnimation(
                ShapeComponent current, Action<Color> set, BackgroundColor color)
            {
                var from = current.Color.Definition.Color;
                var to = Colors.Get(color);
                return AnimationFunction.ColorFromTo(0.1.S()).WithState((set, from, to));
            }
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

            public static readonly TimeSpan NewEntryBackgroundAnimationDuration = 0.4.S();
            public static readonly AnimationFunction<(BackgroundBox, Color)> NewEntryBackgroundAnimation
                = AnimationFunction.BackgroundBoxColorFromTo(NewEntryBackgroundAnimationDuration)
                    .Substitute((BackgroundBox control, Color color) => (control, color * 0.33f, Color.Transparent));
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

            public static readonly AnimationFunction<Control> SlideInAnimation
                = AnimationFunction.ZeroToOne(0.5.S(), (Control control, float t) => control.Anchor(
                    a => a.Right(margin: -Width * (1 - Hermite(0, 0.42f, 1, 0, t)))
                ));

            public static readonly ShapeComponents DefaultBackgroundComponents =
            [
                Fill.With(Colors.Get(BackgroundColor.Default) * 0.8f),
                Edge.Outer(1, Colors.Get(BackgroundColor.MenuOutline)),
                Glow.Outer(ShadowWidth, ShadowColor),
            ];
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
            public const float HeaderLineHeight = 36;

            public const float SubHeaderFontSize = 20;
            public const float SubHeaderLineHeight = 24;

            public const float FontSize = 16;
            public const float LineHeight = 20;

            public const double LabelInBackgroundMargin = LayoutMarginSmall;

            public static readonly Color HeaderColor = Colors.Get(ForeGroundColor.Headline);

            public static readonly Color TextColor = Colors.Get(ForeGroundColor.Text);
            public static readonly Color ErrorTextColor = Color.Red;
            public static readonly Color DisabledTextColor = Colors.Get(ForeGroundColor.DisabledText);
        }

        public static class Tooltip
        {
            public const double DefaultWidth = 300;
            public const double Margin = 4;
            public const double AnchorMargin = 4;

            public static readonly ShapeComponents Background = [
                Fill.With(ShapeColor.From(
                    [(0, Colors.Get(BackgroundColor.Tooltip)), (1, Color.Transparent)],
                    GradientDefinition.Linear(AnchorPoint.Relative((0, 0)), AnchorPoint.Relative((0.9f, 0)))
                )),
                Edge.Outer(1, ShapeColor.From(
                    [(0, Colors.Get(BackgroundColor.TooltipOutline)), (1, Color.Transparent)],
                    GradientDefinition.Linear(AnchorPoint.Relative((0, 0)), AnchorPoint.Relative((0.85f, 0)))
                )),
            ];
        }

        public static class BuildingStatus
        {
            public const double ButtonSize = 32;

            public const double Padding = LayoutMargin;
            public const double EdgeWidth = 1;

            public static class Veterancy
            {
                public const double RowHeight = 24;
                public const double LevelIconSize = 16;
                public const double ExperienceBarHeight = 8;

                public const double ExperienceBarCornerRadius = 2;

                public static readonly Color ExperienceColor = Colors.Experience;
                public static readonly Color NewExperienceColor = Colors.Experience * 0.5f;

                public static readonly ShapeComponents ExperienceBarStaticComponents = [
                    Edge.Outer(1, Colors.Get(BackgroundColor.WindowInsetLine)),
                ];
            }

            public static readonly ShapeComponents StatusRowBackground = [
                Fill.With(ShapeColor.From(
                    [
                        (0, Colors.Get(BackgroundColor.ActiveElement) * 0.5f),
                        (0.95, Color.Transparent),
                    ],
                    GradientDefinition.Linear(AnchorPoint.Relative((0, 0)), AnchorPoint.Relative((0.9f, 0)))
                )),
            ];
            public static readonly double StatusRowBackgroundLeftMargin = EdgeWidth - Padding;

            public static readonly ShapeComponents Background = [
                Fill.With(ShapeColor.From(
                    [
                        (0, Colors.Get(BackgroundColor.Default)),
                        (0.75, Colors.Get(BackgroundColor.Default) * 0.5f),
                        (0.95, Color.Transparent),
                    ],
                    GradientDefinition.Linear(AnchorPoint.Relative((0, 0)), AnchorPoint.Relative((0.9f, 0)))
                )),
                Edge.Inner((float)EdgeWidth, ShapeColor.From(
                    [(0, Colors.Get(BackgroundColor.TooltipOutline)), (0.75, Color.Transparent)],
                    GradientDefinition.Linear(AnchorPoint.Relative((0, 0)), AnchorPoint.Relative((0.9f, 0)))
                )),
            ];
        }

        public static class Window
        {
            public const double TitlebarHeight = NavBar.Height;

            public const float CornerRadius = 4;

            public static readonly ShapeComponents BackgroundComponents = [
                Fill.With(ShapeColor.From(
                    [
                        (0, Colors.Get(BackgroundColor.WindowBackground) * 0.9f),
                        (1, Colors.Get(BackgroundColor.Default) * 0.9f),
                    ],
                    GradientDefinition.Linear(AnchorPoint.Relative((0.3f, 0)), AnchorPoint.Relative((0.6f, 1)))
                )),
                Edge.Inner(1, Colors.Get(BackgroundColor.WindowOutline)),
            ];

            public static readonly Shadow Shadow = Shadows.LargeWindow;
        }
    }
}
