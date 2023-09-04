namespace Bearded.TD.Utilities;

static class StringExtensions
{
    public static string ToCamelCase(this string s) => s.Length == 0 ? s : char.ToLowerInvariant(s[0]) + s[1..];
}
