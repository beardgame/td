using System;
using Bearded.TD.Utilities;
using Bearded.UI.EventArgs;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Controls;

sealed class GameUIController
{
    public Binding<bool> GameMenuVisibility { get; } = new();
    private bool gameMenuVisible => GameMenuVisibility.Value;

    public Binding<bool> NonDiegeticUIVisibility { get; }
    private readonly Binding<bool> nonDiegeticUIVisibility = new(true);

    private readonly Binding<OpenEntityStatus?> openEntityStatus = new();
    private bool entityStatusOpen => openEntityStatus.Value.HasValue;
    public Binding<bool> ActionBarVisibility { get; }

    public Binding<bool> TechnologyModalVisibility { get; } = new();
    private bool technologyModalVisible => TechnologyModalVisibility.Value;

    public GameUIController()
    {
        NonDiegeticUIVisibility = Binding.Combine(
            nonDiegeticUIVisibility,
            GameMenuVisibility,
            (uiVisible, menuVisible) => uiVisible && !menuVisible);
        ActionBarVisibility = openEntityStatus.Transform(openEntity => !openEntity.HasValue);
    }

    public bool TryHandleKeyHit(KeyEventArgs args)
    {
        switch (args.Key)
        {
            case Keys.T:
                toggleTechnologyModal();
                return true;
            case Keys.Escape:
                if (hideAllModals())
                {
                    return true;
                }

                toggleGameMenu();
                return true;
            default:
                return false;
        }
    }

    private void toggleTechnologyModal()
    {
        if (technologyModalVisible)
        {
            HideTechnologyModal();
        }
        else
        {
            ShowTechnologyModal();
        }
    }

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

    private bool hideAllModals()
    {
        var anyModalHidden = false;
        anyModalHidden |= HideTechnologyModal();
        anyModalHidden |= closeEntityStatus();
        return anyModalHidden;
    }

    public bool HideTechnologyModal()
    {
        if (!technologyModalVisible) return false;
        TechnologyModalVisibility.SetFromSource(false);
        return true;
    }

    private bool closeEntityStatus()
    {
        if (!entityStatusOpen) return false;
        openEntityStatus.Value!.Value.Close();
        HideEntityStatus();
        return true;
    }

    public void HideEntityStatus()
    {
        openEntityStatus.SetFromSource(null);
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
