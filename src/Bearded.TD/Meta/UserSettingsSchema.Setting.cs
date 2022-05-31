using System.Collections.Immutable;
using System.Reflection;

namespace Bearded.TD.Meta;

sealed partial class UserSettingsSchema
{
    public interface ISetting
    {
        public string DisplayName { get; }
    }

    public abstract class SettingBase<T> : ISetting
    {
        private readonly FieldInfo group;
        private readonly FieldInfo setting;

        public string DisplayName { get; }

        public T Value
        {
            get
            {
                var settingsInstance = UserSettings.Instance;
                var groupInstance = group.GetValue(settingsInstance);
                return (T)setting.GetValue(groupInstance);
            }
            set
            {
                var settingsInstance = UserSettings.Instance;
                var groupInstance = group.GetValue(settingsInstance);
                setting.SetValue(groupInstance, value);
                UserSettings.RaiseSettingsChanged();
            }
        }

        protected SettingBase(string displayName, FieldInfo group, FieldInfo setting)
        {
            DisplayName = displayName;
            this.group = group;
            this.setting = setting;
        }
    }

    public sealed class BoolSetting : SettingBase<bool>
    {
        public BoolSetting(string displayName, FieldInfo group, FieldInfo setting) :
            base(displayName, group, setting) { }
    }

    public sealed class SelectSetting : SettingBase<object>
    {
        public ImmutableArray<Option> Options { get; }
        private readonly ImmutableDictionary<object, Option> valueToOption;

        public Option? SelectedOption
        {
            get => valueToOption.TryGetValue(Value, out var option) ? option : null;
            set => Value = value?.Value;
        }

        public SelectSetting(
            string displayName,
            ImmutableArray<Option> options,
            FieldInfo group,
            FieldInfo setting) :
            base(displayName, group, setting)
        {
            Options = options;
            valueToOption = options.ToImmutableDictionary(o => o.Value, o => o);
        }

        public sealed record Option(string DisplayName, object Value);
    }

    public sealed class TextSetting : SettingBase<string>
    {
        public TextSetting(string displayName, FieldInfo group, FieldInfo setting) :
            base(displayName, group, setting) { }
    }
}
