using System;
using System.IO;
using Bearded.Utilities;
using Newtonsoft.Json;

namespace Bearded.TD.Meta
{
    sealed class UserSettings
    {
        public static UserSettings Instance { get; private set; }

        #region Loading
        private static readonly JsonSerializer serializer = makeSerializer();

        private static JsonSerializer makeSerializer()
        {
            var s = new JsonSerializer();
            s.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            s.Formatting = Formatting.Indented;
            return s;
        }

        public static void Load(Logger logger)
        {
            logger.Trace.Log($"Loading settings from settings file: {Constants.Paths.UserSettingsFile}");

            try
            {
                using (var reader = File.OpenText(Constants.Paths.UserSettingsFile))
                {
                    Instance = serializer.Deserialize<UserSettings>(new JsonTextReader(reader));
                }

            }
            catch (Exception e)
            {
                logger.Warning.Log($"Could not load user settings: \"{e.Message}\"");
                logger.Info.Log("Loading default settings.");
                Instance = getDefaultInstance();
            }
        }

        private static UserSettings getDefaultInstance()
        {
            return new UserSettings();
        }
        #endregion
    }
}
