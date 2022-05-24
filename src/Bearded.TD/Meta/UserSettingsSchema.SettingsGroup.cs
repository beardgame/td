using System.Collections.Immutable;

namespace Bearded.TD.Meta;

sealed partial class UserSettingsSchema
{
    public sealed record SettingsGroup(string DisplayName, ImmutableArray<ISetting> Settings);
}
