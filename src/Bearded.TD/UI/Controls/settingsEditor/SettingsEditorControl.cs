using System;
using Bearded.TD.Meta;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed class SettingsEditorControl : CompositeControl
{
    public SettingsEditorControl(SettingsEditor model)
    {
        var tabControl = new SettingsTabControl();

        this.BuildLayout()
            .ForFullScreen()
            .AddNavBar(b => b
                .WithBackButton("Back to menu", model.OnBackToMenuButtonClicked))
            .AddTabs(t =>
            {
                foreach (var group in model.SettingsGroups)
                {
                    t.AddButton(group.DisplayName, () => tabControl.Populate(group));
                }
                return t;
            })
            .FillContent(tabControl);
    }

    private sealed class SettingsTabControl : CompositeControl
    {
        public void Populate(UserSettingsSchema.SettingsGroup group)
        {
            RemoveAllChildren();
            this.BuildLayout().FillContent(FormFactories.Form(form =>
            {
                foreach (var setting in group.Settings)
                {
                    switch (setting)
                    {
                        case UserSettingsSchema.BoolSetting boolSetting:
                            var boolBinding =
                                Binding.Create(boolSetting.Value, newValue => boolSetting.Value = newValue);
                            form.AddCheckboxRow(boolSetting.DisplayName, boolBinding);
                            break;
                        case UserSettingsSchema.SelectSetting selectSetting:
                            var selectBinding = Binding.Create(
                                selectSetting.SelectedOption, option => selectSetting.SelectedOption = option);
                            form.AddDropdownSelectRow(selectSetting.DisplayName,
                                selectSetting.Options,
                                option => option.DisplayName,
                                selectBinding);
                            break;
                        case UserSettingsSchema.TextSetting textSetting:
                            var textBinding =
                                Binding.Create(textSetting.Value, newValue => textSetting.Value = newValue);
                            form.AddTextInputRow(textSetting.DisplayName, textBinding);
                            break;
                        default:
                            throw new NotSupportedException($"Setting type not supported: {setting}");
                    }
                }

                return form;
            }));
        }
    }
}
