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
        }
    }
}