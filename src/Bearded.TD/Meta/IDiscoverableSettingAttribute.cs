using System;

namespace Bearded.TD.Meta;

interface IDiscoverableSettingAttribute
{
    string? DisplayName { get; }
}

[AttributeUsage(AttributeTargets.Field)]
sealed class DiscoverableBoolSettingAttribute : Attribute, IDiscoverableSettingAttribute
{
    public string? DisplayName { get; set; }
}

[AttributeUsage(AttributeTargets.Field)]
sealed class DiscoverableToggleSettingAttribute : Attribute, IDiscoverableSettingAttribute
{
    public string? DisplayName { get; set; }
    public string EnabledDisplayText { get; set; } = "Enabled";
    public string DisabledDisplayText { get; set; } = "Disabled";
}

[AttributeUsage(AttributeTargets.Field)]
sealed class DiscoverableSelectSettingAttribute : Attribute, IDiscoverableSettingAttribute
{
    public string? DisplayName { get; set; }
    public object[] Options { get; set; } = { };
    public string?[]? OptionNames { get; set; } = { };
}

[AttributeUsage(AttributeTargets.Field)]
sealed class DiscoverableTextSettingAttribute : Attribute, IDiscoverableSettingAttribute
{
    public string? DisplayName { get; set; }
}
