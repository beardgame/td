using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.UI;

static class Formatters
{
    public static string ToDisplayString(this TimeSpan timeSpan) =>
        System.TimeSpan.FromSeconds(timeSpan.NumericValue).ToString("m\\:ss");
}