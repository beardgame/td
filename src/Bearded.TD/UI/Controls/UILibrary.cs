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
        RenderContext renderContext, Animations animations)
    {
        // Please keep alphabetically sorted.
        return NavigationFactories.ForBoth()
            .Add<DebugConsole, Void>(m => new DebugConsoleControl(m, animations))
            .Add<GameDebugOverlay, Void>(m => new GameDebugOverlayControl(m))
            .Add<GameUI, GameUI.Parameters>(m => new GameUIControl(m, renderContext))
            .Add<LoadingScreen, LoadingManager>(m => new LoadingScreenControl(m))
            .Add<Lobby, LobbyManager>(m => new LobbyControl(m))
            .Add<LobbyList, Void>(m => new LobbyListControl(m))
            .Add<MainMenu, Intent>(m => new MainMenuControl(m, animations))
            .Add<PerformanceOverlay, Void>(m => new PerformanceOverlayControl(m))
            .Add<SettingsEditor, Void>(m => new SettingsEditorControl(m))
            .Add<FontTest, Void>(m => new FontTextControl(m))
            .Add<UIDebugOverlay, Void>(m => new UIDebugOverlayControl(m))
            .Add<VersionOverlay, Void>(m => new VersionOverlayControl(m))
            .ToDictionaries();
    }
}
