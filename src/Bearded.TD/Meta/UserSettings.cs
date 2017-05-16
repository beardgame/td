using System;
using System.IO;
using Bearded.TD.Utilities.Console;
using Bearded.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Bearded.TD.Meta
{
    sealed class UserSettings
    {
        public static UserSettings Instance { get; private set; }

        #region I/O
        private static readonly JsonSerializer serializer = makeSerializer();

        private static JsonSerializer makeSerializer()
        {
            var s = new JsonSerializer();
            s.Converters.Add(new StringEnumConverter());
            s.ContractResolver = new CamelCasePropertyNamesContractResolver();
            s.Formatting = Formatting.Indented;
            return s;
        }

        public static void Load(Logger logger)
        {
            logger.Trace.Log($"Attempting to load settings from settings file: {Constants.Paths.UserSettingsFile}");

            try
            {
                using (var reader = File.OpenText(Constants.Paths.UserSettingsFile))
                {
                    Instance = serializer.Deserialize<UserSettings>(new JsonTextReader(reader));
                }
                logger.Trace.Log("Finished loading user settings.");
            }
            catch (Exception e)
            {
                logger.Warning.Log($"Could not load user settings: {e.Message}");
                logger.Info.Log("Loading default settings.");
                Instance = getDefaultInstance();
            }
        }

        public static bool Save(Logger logger)
        {
            logger.Trace.Log($"Attempting to save settings to settings file: {Constants.Paths.UserSettingsFile}");

            try
            {
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, Instance);

                    var fileName = Constants.Paths.UserSettingsFile;
                    var dirName = Path.GetDirectoryName(fileName);

                    // ReSharper disable AssignNullToNotNullAttribute
                    if (!Directory.Exists(dirName))
                        Directory.CreateDirectory(dirName);
                    // ReSharper restore AssignNullToNotNullAttribute
                    File.WriteAllText(fileName, writer.ToString());
                }
                logger.Trace.Log("Finished saving user settings.");
                return true;
            }
            catch (Exception e)
            {
                logger.Warning.Log($"Could not save user settings: {e.Message}");
            }
            return false;
        }

        private static UserSettings getDefaultInstance()
        {
            return new UserSettings();
        }
        #endregion

        #region Console
        [Command("setting")]
        private static void setSetting(Logger logger, CommandParameters p)
        {
            if (p.Args.Length != 2)
            {
                logger.Warning.Log("Usage: \"setting [setting_name] [setting_value]\"");
                return;
            }

            // Convert to JSON.
            var splitSettingName = p.Args[0].Split('.');
            (var jsonBefore, var jsonAfter) = buildJson(splitSettingName, 0);
            var json = jsonBefore + p.Args[1] + jsonAfter;
            serializer.Populate(new StringReader(json), Instance);
            Save(logger);
        }

        private static (string, string) buildJson(string[] parts, int i)
        {
            if (i >= parts.Length) return ("", "");
            (var before, var after) = buildJson(parts, i + 1);
            return ($"{{ \"{parts[i]}\": {before}", $"{after} }}");
        }
        #endregion

        public UISettings UI = new UISettings();

        public class UISettings
        {
            public float UIScale = 1f;
        }
    }
}
