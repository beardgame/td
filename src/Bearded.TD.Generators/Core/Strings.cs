namespace Bearded.TD.Generators;

static class Strings
{
    // We cannot use System.Environment in analyzer context, so we use a raw string to fetch the newline with which the
    // repository was checked out to get an approximation of the new line character we should use in generated code.
    // ReSharper disable once UseRawString
    public const string NewLine = @"
";
}
