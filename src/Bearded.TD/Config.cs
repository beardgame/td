namespace Bearded.TD
{
    static class Config
    {
        public const string BaseVersionString = "0.1";

#if DEBUG
        private const string versionPostFix = "-dev";
#else
        private const string versionPostFix = "";
#endif

        public const string VersionString = BaseVersionString + versionPostFix;
    }
}
