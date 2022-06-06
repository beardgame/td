using System.Collections.Immutable;
using Bearded.Graphics.Text;

namespace Bearded.TD.UI;

sealed class LineWrapper
{
    private readonly Font font;

    public LineWrapper(Font font)
    {
        this.font = font;
    }

    public ImmutableArray<string> SplitIntoLines(string str, float fontSize, float maxLineWidth)
    {
        var normalizedMaxLineWidth = maxLineWidth / fontSize;
        var words = str.Split();

        var builder = ImmutableArray.CreateBuilder<string>();
        var currLine = "";

        foreach (var w in words)
        {
            var concatenatedLine = currLine + " " + w;
            if (font.StringWidth(concatenatedLine) <= normalizedMaxLineWidth)
            {
                currLine = concatenatedLine;
                continue;
            }

            if (currLine.Length > 0)
            {
                builder.Add(currLine);
            }

            currLine = w;
        }

        if (currLine.Length > 0)
        {
            builder.Add(currLine);
        }

        return builder.ToImmutable();
    }
}
