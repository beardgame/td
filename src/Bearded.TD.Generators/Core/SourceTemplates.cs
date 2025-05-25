using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bearded.TD.Generators;

static class SourceTemplates
{
    public static string Foreach<T>(
        int indents, IEnumerable<T> values, Func<T, string> template, string separator = "\n")
    {
        return string.Join($"{separator}{Indent(indents)}", values.Select(template));
    }

    public static string Indent(int i) => new(' ', i * 4);

    public static string CleanWhiteSpace(string source)
    {
        // strip trailing whitespace
        source = Regex.Replace(source, @"[ \t]+(\r?\n)", "$1");

        // remove multiple consecutive empty lines
        source = Regex.Replace(source, @"(\r?\n){3,}", "$1$1");

        // remove empty lines after { or ]
        source = Regex.Replace(source, @"([\{\]])(\r?\n){2}", "$1$2");

        return source;
    }
}
