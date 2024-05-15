using System;
using System.Collections.Generic;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Animation;
using Bearded.UI.Navigation;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Controls;

static class UILibrary
{
    public static (IDictionary<Type, object> models, IDictionary<Type, object> views) CreateFactories(
        RenderContext renderContext, UIContext uiContext)
    {
        // Please keep alphabetically sorted.
        return NavigationFactories.ForBoth()
            .Add<DebugConsole, Void>(m => new DebugConsoleControl(m, uiContext))
            .Add<GameDebugOverlay, Void>(m => new GameDebugOverlayControl(m, uiContext))
            .Add<GameUI, GameUI.Parameters>(m => new GameUIControl(m, renderContext, uiContext))
            .Add<LoadingScreen, LoadingManager>(m => new LoadingScreenControl(m))
            .Add<Lobby, LobbyManager>(m => new LobbyControl(m, uiContext))
            .Add<LobbyList, Void>(m => new LobbyListControl(m, uiContext))
            .Add<MainMenu, Intent>(m => new MainMenuControl(m, uiContext))
            .Add<PerformanceOverlay, Void>(m => new PerformanceOverlayControl(m))
            .Add<SettingsEditor, Void>(m => new SettingsEditorControl(m, uiContext))
            .Add<FontTest, Void>(m => new FontTextControl(m, uiContext))
            .Add<UIDebugOverlay, Void>(m => new UIDebugOverlayControl(m, uiContext))
            .Add<VersionOverlay, Void>(m => new VersionOverlayControl(m))
            .ToDictionaries();
    }
}
