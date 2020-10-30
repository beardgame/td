using System.Collections.Immutable;

namespace Bearded.TD.Generators.TechEffects
{
    static class TechEffectsUtils
    {
        private static readonly ImmutableHashSet<string> reservedNames = ImmutableHashSet.Create("object");

        public static string toCamelCase(string str)
        {
            var camelCaseStr = char.ToLowerInvariant(str[0]) + str.Substring(1);
            return reservedNames.Contains(camelCaseStr) ? $"@{camelCaseStr}" : camelCaseStr;
        }
    }
}
