using System;
using Bearded.TD.Meta;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed class SettingsEditorControl : CompositeControl
{
    private readonly Binding<UserSettingsSchema.SettingsGroup?> activeGroup =
        Binding.Create<UserSettingsSchema.SettingsGroup?>(null);

    public SettingsEditorControl(SettingsEditor model, UIContext uiContext)
    {
        var tabControl = new SettingsTabControl(uiContext);

        var factories = uiContext.Factories;

        this.BuildLayout()
            .ForFullScreen()
            .AddNavBar(factories, b => b
                .WithBackButton("Back to menu", model.OnBackToMenuButtonClicked))
            .AddTabs(factories, t =>
            {
                foreach (var group in model.SettingsGroups)
                {
                    t.AddButton(
                        group.DisplayName,
                        () => selectGroup(group),
                        activeGroup.Transform(g => Equals(g, group)));
                }
                return t;
            })
            .FillContent(tabControl);

        void selectGroup(UserSettingsSchema.SettingsGroup group)
        {
            tabControl.Populate(group);
            activeGroup.SetFromSource(group);
        }
    }

    private sealed class SettingsTabControl(UIContext uiContext) : CompositeControl
    {
        public void Populate(UserSettingsSchema.SettingsGroup group)
        {
            RemoveAllChildren();
            this.BuildLayout().FillContent(uiContext.Factories.Form(form =>
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
                            form.AddButtonSelectRow(selectSetting.DisplayName,
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
