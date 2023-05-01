using System;
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

    private readonly Binding<OpenEntityStatus?> openEntityStatus = new();
    private bool entityStatusOpen => openEntityStatus.Value.HasValue;
    public IReadonlyBinding<bool> ActionBarVisibility { get; }

    public Binding<bool> TechnologyModalVisibility { get; } = new();

    public ShortcutLayer Shortcuts { get; }

    public GameUIController()
    {
        NonDiegeticUIVisibility = Binding.Combine(
            nonDiegeticUIVisibility,
            GameMenuVisibility,
            (uiVisible, menuVisible) => uiVisible && !menuVisible);
        ActionBarVisibility = openEntityStatus.Transform(openEntity => !openEntity.HasValue);
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

    public void ShowEntityStatus(OpenEntityStatus entityStatus)
    {
        hideAllModals();
        openEntityStatus.SetFromSource(entityStatus);
    }

    private void showGameMenu()
    {
        hideAllModals();
        GameMenuVisibility.SetFromSource(true);
    }

    private void hideAllModals()
    {
        hideTechnologyModal();
        closeEntityStatus();
    }

    private void hideTechnologyModal()
    {
        TechnologyModalVisibility.SetFromSource(false);
    }

    private void closeEntityStatus()
    {
        if (!entityStatusOpen) return;
        openEntityStatus.Value!.Value.Close();
        HideEntityStatus();
    }

    public void HideEntityStatus()
    {
        openEntityStatus.SetFromSource(null);
    }

    public void ResumeGame()
    {
        hideGameMenu();
    }

    private void hideGameMenu()
    {
        GameMenuVisibility.SetFromSource(false);
    }

    public readonly struct OpenEntityStatus
    {
        private readonly Action close;

        public OpenEntityStatus(Action close)
        {
            this.close = close;
        }

        public void Close() => close();
    }
}
