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
        private const string settingParameterCompletionName = "allSettingStrings";
        private static readonly Assembly thisAssembly = typeof(UserSettings).Assembly;
        private static readonly char[] lineEndingChars = {'\n', '\r'};

        [CommandParameterCompletion(settingParameterCompletionName)]
        public static IEnumerable<string> CommandParameters()
        {
            return getFieldsAndPropertiesOf(typeof(UserSettings));
        }

        private static IEnumerable<string> getFieldsAndPropertiesOf(Type type)
        {
            return type.GetMembers()
                .Where(m => m is FieldInfo || m is PropertyInfo)
                .Select(m => (Name: m.Name.ToLower(), Type: fieldOrPropertyType(m)))
                .Where(m => m.Type != typeof(UserSettings))
                .SelectMany(m =>
                {
                    var (memberName, memberType) = m;

                    if (memberType.Assembly != thisAssembly || memberType.IsEnum || memberType.IsValueType)
                        return new[] {memberName};

                    return getFieldsAndPropertiesOf(memberType).Select(suffix => $"{memberName}.{suffix}");
                });
        }

        private static Type fieldOrPropertyType(MemberInfo memberInfo) =>
            memberInfo switch
            {
                FieldInfo f => f.FieldType,
                PropertyInfo p => p.PropertyType,
                _ => throw new InvalidOperationException()
            };

        [Command("setting", settingParameterCompletionName)]
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
                    logger.Warning?.Log("Usage: \"setting <setting_name> [<setting_value>]\"");
                    break;
            }
        }

        private static void getSetting(Logger logger, CommandParameters p)
        {
            var splitSettingName = p.Args[0].Split('.');

            object currentObject = Instance;

            foreach (var name in splitSettingName)
            {
                var member = currentObject.GetType().GetMember(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                    .FirstOrDefault();

                currentObject = member switch
                {
                    FieldInfo field => field.GetValue(currentObject),
                    PropertyInfo property => property.GetValue(currentObject),
                    _ => null
                };

                if (currentObject == null)
                {
                    logger.Warning?.Log($"Could not find setting path part '{name}'");
                    return;
                }
            }

            var writer = new StringWriter();
            serializer.Serialize(writer, currentObject);
            var jsonLines = writer.ToString().Split(lineEndingChars, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in jsonLines)
            {
                logger.Info?.Log(line);
            }
        }

        private static void setSetting(Logger logger, CommandParameters p)
        {
            var splitSettingName = p.Args[0].Split('.');
            var (jsonBefore, jsonAfter) = buildJson(splitSettingName, 0);
            var json = jsonBefore + p.Args[1] + jsonAfter;
            try
            {
                serializer.Populate(new StringReader(json), Instance);
                SettingsChanged?.Invoke();
            }
            catch (JsonReaderException e)
            {
                logger.Warning?.Log($"Encountered error parsing setting: {e.Message}");
                return;
            }

            Save(logger);
        }

        private static (string, string) buildJson(string[] parts, int i)
        {
            if (i >= parts.Length) return ("", "");
            var (before, after) = buildJson(parts, i + 1);
            return ($"{{ \"{parts[i]}\": {before}", $"{after} }}");
        }
    }
}
