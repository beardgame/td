using Bearded.TD.UI.Shortcuts;
using Bearded.TD.Utilities;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Controls;

sealed class GameUIController
{
    public Binding<bool> GameMenuVisibility { get; } = new();
    private bool gameMenuVisible => GameMenuVisibility.Value;

    public IReadonlyBinding<bool> NonDiegeticUIVisibility { get; }
    private readonly Binding<bool> nonDiegeticUIVisibility = new(true);

    public Binding<bool> TechnologyModalVisibility { get; } = new();

    public ShortcutLayer Shortcuts { get; }

    public GameUIController()
    {
        NonDiegeticUIVisibility = Binding.Combine(
            nonDiegeticUIVisibility,
            GameMenuVisibility,
            (uiVisible, menuVisible) => uiVisible && !menuVisible);
        Shortcuts = buildShortcuts();
    }

    private ShortcutLayer buildShortcuts() => ShortcutLayer.CreateBuilder()
        .AddShortcut(Keys.Escape, toggleGameMenu)
        .Build();

    private void toggleGameMenu()
    {
        if (gameMenuVisible)
        {
            hideGameMenu();
        }
        else
        {
            showGameMenu();
        }
    }

    public void ShowTechnologyModal()
    {
        hideAllModals();
        TechnologyModalVisibility.SetFromSource(true);
    }

    private void showGameMenu()
    {
        hideAllModals();
        GameMenuVisibility.SetFromSource(true);
    }

    private void hideAllModals()
    {
        hideTechnologyModal();
    }

    private void hideTechnologyModal()
    {
        TechnologyModalVisibility.SetFromSource(false);
    }

    public void ResumeGame()
    {
        hideGameMenu();
    }

    private void hideGameMenu()
    {
        GameMenuVisibility.SetFromSource(false);
    }
}
