using System;
using Bearded.TD.Tiles;
using Bearded.TD.UI;
using Bearded.TD.UI.Controls;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.UI;

sealed class AnimatedNumberLabelRenderer(UIFonts uiFonts) : IRenderer<AnimatedNumberLabel>
{
    public void Render(AnimatedNumberLabel label)
    {
        var argb = label.Color;

        if (label.Parent is Button { IsEnabled: false })
        {
            argb *= 0.5f;
        }

        var textAnchor = (Vector2)label.TextAnchor;
        var frame = label.Frame;
        var anchor = frame.TopLeft + frame.Size * textAnchor;
        var fontSize = (float)label.FontSize;

        var drawer = uiFonts.ForStyle(label.TextStyle);

        var value = label.CurrentValue;

        if (!label.Config.RenderZero && value == 0)
            return;

        var absoluteValue = Math.Abs(value);

        var flatDigits = MoreMath.CeilToInt(absoluteValue).ToString();
        var digitCount = flatDigits.Length;
        var charWidth = drawer.StringWidth("0", fontSize).X;
        var flatWidth = charWidth * digitCount;

        if (value < 0)
        {
            flatWidth += drawer.StringWidth("-", fontSize).X;
        }

        var right = anchor.X + (1 - textAnchor.X) * flatWidth;

        var currentPowerOfTen = 1;

        var fraction = (float)(value - Math.Floor(value));

        var scrollDirectionSign = label.Config.FlipScrollDirection ? 1 : -1;
        var maxScrollOffset = fontSize * scrollDirectionSign;
        var scrollOffset = fraction * maxScrollOffset;

        var hasLeadingZero = false;

        for (var i = 0; i < digitCount; i++)
        {
            var modulo = currentPowerOfTen * 10;

            var lowerDigit = Math.Abs(MoreMath.FloorToInt(value) % modulo / currentPowerOfTen);
            var upperDigit = Math.Abs(MoreMath.CeilToInt(value) % modulo / currentPowerOfTen);

            if (lowerDigit == upperDigit)
            {
                drawer.DrawLine(
                    xyz: (Vector3)new Vector3d(right, anchor.Y, 0),
                    text: lowerDigit.ToString(),
                    alignHorizontal: 1,
                    alignVertical: textAnchor.Y,
                    fontHeight: fontSize,
                    parameters: argb
                );
            }
            else
            {
                var isLargestDigit = (i > 0 || !label.Config.RenderZero) && i == digitCount - 1;
                var lowerIsLeadingZero = isLargestDigit && lowerDigit == 0;
                var upperIsLeadingZero = isLargestDigit && upperDigit == 0;

                hasLeadingZero |= lowerIsLeadingZero || upperIsLeadingZero;

                var lowerAlpha = 1 - fraction;
                var upperAlpha = fraction;

                if (!lowerIsLeadingZero)
                {
                    drawer.DrawLine(
                        xyz: (Vector3)new Vector3d(right, anchor.Y + scrollOffset, 0),
                        text: lowerDigit.ToString(),
                        alignHorizontal: 1,
                        alignVertical: textAnchor.Y,
                        fontHeight: fontSize,
                        parameters: argb * lowerAlpha
                    );
                }

                if (!upperIsLeadingZero)
                {
                    drawer.DrawLine(
                        xyz: (Vector3)new Vector3d(right, anchor.Y + scrollOffset - maxScrollOffset, 0),
                        text: upperDigit.ToString(),
                        alignHorizontal: 1,
                        alignVertical: textAnchor.Y,
                        fontHeight: fontSize,
                        parameters: argb * upperAlpha
                    );
                }
            }

            right -= charWidth;
            currentPowerOfTen *= 10;
        }

        if (label.Config.ShowPlusSign || value < 0)
        {
            var sign = value < 0 ? "-" : "+";
            var zeroToOne = (float)Math.Min(Math.Abs(value), 1);
            var direction = value < 0 ? 1 : -1;
            var signOffset = (1 - zeroToOne) * maxScrollOffset * direction;

            if (zeroToOne >= 1 && hasLeadingZero)
            {
                right += charWidth * (value < 0 ? fraction : 1 - fraction);
            }

            drawer.DrawLine(
                xyz: (Vector3)new Vector3d(right, anchor.Y + signOffset, 0),
                text: sign,
                alignHorizontal: 1,
                alignVertical: textAnchor.Y,
                fontHeight: fontSize,
                parameters: argb * zeroToOne
            );
        }
    }
}
