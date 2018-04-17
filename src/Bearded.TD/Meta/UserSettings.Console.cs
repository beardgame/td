using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bearded.TD.Utilities.Console;
using Bearded.Utilities.IO;
using Newtonsoft.Json;

namespace Bearded.TD.Meta
{
    sealed partial class UserSettings
    {
        private static readonly Assembly thisAssembly = typeof(UserSettings).Assembly;
        private static readonly char[] lineEndingChars = {'\n', '\r'};

        private static void initialiseCommandParameters()
        {
            var allParameters = getFieldsOf(typeof(UserSettings)).ToList();

            ConsoleCommands.AddParameterCompletion("allSettingStrings", allParameters);
        }

        private static IEnumerable<string> getFieldsOf(Type type)
        {
            return type.GetFields()
                .SelectMany(field =>
                {
                    var fieldName = field.Name.ToLower();
                    var fieldType = field.FieldType;

                    if (fieldType.Assembly != thisAssembly)
                        return new[] {fieldName};

                    return getFieldsOf(fieldType).Select(suffix => $"{fieldName}.{suffix}");
                });
        }

        [Command("setting", "allSettingStrings")]
        private static void settingCommand(Logger logger, CommandParameters p)
        {
            switch (p.Args.Length)
            {
                case 1:
                    getSetting(logger, p);
                    break;
                case 2:
                    setSetting(logger, p);
                    break;
                default:
                    logger.Warning.Log("Usage: \"setting <setting_name> [<setting_value>]\"");
                    break;
            }
        }

        private static void getSetting(Logger logger, CommandParameters p)
        {
            var splitSettingName = p.Args[0].Split('.');

            object currentObject = Instance;

            foreach (var name in splitSettingName)
            {
                var field = currentObject.GetType().GetField(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

                if (field == null)
                {
                    logger.Warning.Log($"Could not find setting path part '{name}'");
                    return;
                }

                currentObject = field.GetValue(currentObject);
            }

            var jsonLines = JsonConvert
                .SerializeObject(currentObject, Formatting.Indented)
                .Split(lineEndingChars, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in jsonLines)
            {
                logger.Info.Log(line);
            }
        }

        private static void setSetting(Logger logger, CommandParameters p)
        {
            var splitSettingName = p.Args[0].Split('.');
            (var jsonBefore, var jsonAfter) = buildJson(splitSettingName, 0);
            var json = jsonBefore + p.Args[1] + jsonAfter;
            try
            {
                serializer.Populate(new StringReader(json), Instance);
                SettingsChanged?.Invoke();
            }
            catch (JsonReaderException e)
            {
                logger.Warning.Log($"Encountered error parsing setting: {e.Message}");
                return;
            }
            Save(logger);
        }

        private static (string, string) buildJson(string[] parts, int i)
        {
            if (i >= parts.Length) return ("", "");
            (var before, var after) = buildJson(parts, i + 1);
            return ($"{{ \"{parts[i]}\": {before}", $"{after} }}");
        }
    }
}
