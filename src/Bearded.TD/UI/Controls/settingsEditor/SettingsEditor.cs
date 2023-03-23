using System.Collections.Immutable;
using Bearded.TD.Meta;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Controls;

sealed class SettingsEditor : NavigationNode<Void>
{
    private Logger logger = null!;
    public ImmutableArray<UserSettingsSchema.SettingsGroup> SettingsGroups { get; private set; }

    protected override void Initialize(DependencyResolver dependencies, Void _)
    {
        logger = dependencies.Resolve<Logger>();
        SettingsGroups = UserSettingsSchema.Instance.SettingsGroups;
    }

    public void OnBackToMenuButtonClicked()
    {
        UserSettings.Save(logger);
        Navigation!.Replace<MainMenu, Intent>(Intent.None, this);
    }
}
