using System;
using System.Collections.Immutable;

namespace Bearded.TD.Meta;

sealed partial class UserSettingsSchema
{
    private static UserSettingsSchema? instance;

    public static UserSettingsSchema Instance => instance ??
        throw new InvalidOperationException("Cannot access user settings schema before initializing");

    public ImmutableArray<SettingsGroup> SettingsGroups { get; }

    private UserSettingsSchema(ImmutableArray<SettingsGroup> settingsGroups)
    {
        SettingsGroups = settingsGroups;
    }
}
