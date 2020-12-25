using System.IO;
using Bearded.Utilities;

namespace Bearded.TD
{
    static partial class Constants
    {
        public static class Paths
        {
            private static string settingsDirectory;

            public static string SettingsDirectory
            {
                get
                {
                    if (settingsDirectory != null)
                        return settingsDirectory;

                    var dir = Environment.UserSettingsDirectoryFor("Bearded.TD");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    settingsDirectory = dir;
                    return dir;
                }
            }

            public static readonly string UserSettingsFile = SettingsDirectory + "/usersettings.json";
            public static readonly string LogFile = SettingsDirectory + "/debug.log";

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
                    // point at asset files in the actual repo instead of the binary folder for easy live editing

                    // your\td\path
                    // \bin\Bearded.TD\Debug\ -> \src\Bearded.TD\
                    // assets\file.ext

                    var newFile = file
                        .Replace("\\", "/")
                        .Replace("/bin/Bearded.TD/Debug/", "/src/Bearded.TD/");

                    return newFile;
                }
            }
        }
    }
}
