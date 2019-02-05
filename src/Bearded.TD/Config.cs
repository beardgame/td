namespace Bearded.TD
{
    static class Config
    {
        public const string BaseVersionString = "0.2";

#if DEBUG
        private const string versionSuffix = "-dev";
#else
        private const string versionSuffix = "";
#endif

        public static string VersionString { get; } = BaseVersionString + versionSuffix;
    }
}
