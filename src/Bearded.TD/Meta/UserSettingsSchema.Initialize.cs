using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Meta;

sealed partial class UserSettingsSchema
{
    public static void Initialize()
    {
        var settingsGroups = typeof(UserSettings).GetFields()
            .Select(field =>
            {
                var maybeAttribute = field.GetCustomAttribute(typeof(DiscoverableSettingGroupAttribute));

                if (maybeAttribute is not DiscoverableSettingGroupAttribute attribute)
                {
                    return null;
                }

                var group = settingsGroupFromType(field, attribute);
                return group.Settings.IsDefaultOrEmpty ? null : group;

            })
            .NotNull()
            .ToImmutableArray();

        instance = new UserSettingsSchema(settingsGroups);
    }

    private static SettingsGroup settingsGroupFromType(FieldInfo group, DiscoverableSettingGroupAttribute attribute)
    {
        var settings = group.FieldType.GetFields()
            .Select(field =>
            {
                var maybeAttribute =
                    field.GetCustomAttributes().OfType<IDiscoverableSettingAttribute>().SingleOrDefault();

                return maybeAttribute is { } attr
                    ? settingFromField(group, field, attr)
                    : null;
            })
            .NotNull()
            .ToImmutableArray();

        return new SettingsGroup(attribute.DisplayName ?? group.Name, settings);
    }

    private static ISetting settingFromField(
        FieldInfo group, FieldInfo setting, IDiscoverableSettingAttribute attribute)
    {
        var displayName = attribute.DisplayName ?? setting.Name;
        return attribute switch
        {
            DiscoverableBoolSettingAttribute => new BoolSetting(displayName, group, setting),
            DiscoverableToggleSettingAttribute attr => new SelectSetting(
                displayName,
                ImmutableArray.Create(
                    new SelectSetting.Option(attr.EnabledDisplayText, true),
                    new SelectSetting.Option(attr.DisabledDisplayText, false)),
                group,
                setting),
            DiscoverableSelectSettingAttribute attr => new SelectSetting(
                displayName,
                toOptions(attr.Options, attr.OptionNames),
                group,
                setting),
            DiscoverableTextSettingAttribute => new TextSetting(displayName, group, setting),
            _ => throw new NotSupportedException(
                $"Found unsupported discoverable setting {setting.Name} with type {setting.FieldType.Name}")
        };
    }

    private static ImmutableArray<SelectSetting.Option> toOptions(
        IReadOnlyCollection<object> values, IReadOnlyCollection<string?>? names)
    {
        if (names == null || names.Count == 0)
        {
            return values.Select(v => new SelectSetting.Option($"{v}", v)).ToImmutableArray();
        }

        if (names.Count != values.Count)
        {
            throw new InvalidOperationException("Option names must have same length as option values");
        }

        return values.Zip(names)
            .Select(tuple => new SelectSetting.Option(tuple.Second ?? $"{tuple.First}", tuple.First))
            .ToImmutableArray();
    }
}
