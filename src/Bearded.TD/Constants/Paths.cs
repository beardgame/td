using System.IO;
using Bearded.Utilities;

namespace Bearded.TD;

static partial class Constants
{
    public static class Paths
    {
        private static string? settingsDirectory;

        public static string SettingsDirectory => settingsDirectory ??= getValidSettingsDirectory();

        private static string getValidSettingsDirectory()
        {
            var dir = Environment.UserSettingsDirectoryFor("Bearded.TD");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        public static readonly string UserSettingsFile = SettingsDirectory + "/usersettings.json";
        public static readonly string LogFile = SettingsDirectory + "/debug.log";

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static class Content
        {
            private static readonly string workingDir =
                adjustPathToReloadableIfDebug(Directory.GetCurrentDirectory() + "/");

            public static string Asset(string path) => workingDir + "assets/" + path;

            private static string adjustPathToReloadableIfDebug(string file)
            {
#if !DEBUG
                    return file;
#endif

                var newFile = file
                    .Replace("\\", "/")
                    .Replace("/bin/Bearded.TD/Debug/", "/src/Bearded.TD/");

                return newFile;
            }
        }
    }
}