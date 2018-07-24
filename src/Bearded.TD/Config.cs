namespace Bearded.TD
{
    static class Config
    {
        public const string BaseVersionString = "0.1";

#if DEBUG
        private const string versionSuffix = "-dev";
#else
        private const string versionSuffix = "";
#endif

        public const string VersionString = BaseVersionString + versionSuffix;
    }
}
