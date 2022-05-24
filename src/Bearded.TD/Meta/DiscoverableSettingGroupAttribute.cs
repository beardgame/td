using System;

namespace Bearded.TD.Meta;

[AttributeUsage(AttributeTargets.Field)]
sealed class DiscoverableSettingGroupAttribute : Attribute
{
    public string? DisplayName { get; set; }
}
