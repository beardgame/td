using System;
using System.IO;
using System.Text.Json;
using Bearded.Utilities.IO;

namespace Bearded.TD.Meta
{
    sealed partial class UserSettings
    {
        private static readonly JsonSerializerOptions serializerOptions = makeSerializerOptions();

        private static JsonSerializerOptions makeSerializerOptions()
        {
            var s = new JsonSerializerOptions(Constants.Serialization.DefaultJsonSerializerOptions)
            {
                Converters = {Constants.Serialization.StringEnumConverter}
            };
            return s;
        }

        public static void Load(Logger logger)
        {
            logger.Trace?.Log($"Attempting to load settings from settings file: {Constants.Paths.UserSettingsFile}");

            try
            {
                using (var reader = File.OpenText(Constants.Paths.UserSettingsFile))
                {
                    Instance = JsonSerializer.Deserialize<UserSettings>(reader.ReadToEnd(), serializerOptions)
                        ?? throw new InvalidDataException("Could not parse user settings");
                    SettingsChanged?.Invoke();
                }
                logger.Trace?.Log("Finished loading user settings.");
            }
            catch (Exception e)
            {
                logger.Warning?.Log($"Could not load user settings: {e.Message}");
                logger.Info?.Log("Loading default settings.");
                Instance = getDefaultInstance();
            }
        }

        public static bool Save(Logger logger)
        {
            logger.Trace?.Log($"Attempting to save settings to settings file: {Constants.Paths.UserSettingsFile}");

            try
            {
                var jsonString = JsonSerializer.Serialize(Instance, serializerOptions);

                var fileName = Constants.Paths.UserSettingsFile;
                var dirName = Path.GetDirectoryName(fileName);

                // ReSharper disable AssignNullToNotNullAttribute
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
                // ReSharper restore AssignNullToNotNullAttribute
                File.WriteAllText(fileName, jsonString);
                logger.Trace?.Log("Finished saving user settings.");
                return true;
            }
            catch (Exception e)
            {
                logger.Warning?.Log($"Could not save user settings: {e.Message}");
            }
            return false;
        }

        private static UserSettings getDefaultInstance() => new();
    }
}
