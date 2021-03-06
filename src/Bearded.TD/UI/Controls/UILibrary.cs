﻿using System;
using System.Collections.Generic;
using Bearded.TD.Game;
using Bearded.TD.Rendering;
using Bearded.UI.Navigation;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Controls
{
    static class UILibrary
    {
        public static (IDictionary<Type, object> models, IDictionary<Type, object> views) CreateFactories(RenderContext renderContext)
        {
            // Please keep alphabetically sorted.
            return NavigationFactories.ForBoth()
                .Add<DebugConsole, Void>(m => new DebugConsoleControl(m))
                .Add<GameDebugOverlay, Void>(m => new GameDebugOverlayControl(m))
                .Add<GameUI, (GameInstance, GameRunner)>(
                    m => new GameUIControl(m, renderContext))
                .Add<LoadingScreen, LoadingManager>(m => new LoadingScreenControl(m))
                .Add<Lobby, LobbyManager>(m => new LobbyControl(m))
                .Add<LobbyList, Void>(m => new LobbyListControl(m))
                .Add<MainMenu, Void>(m => new MainMenuControl(m))
                .Add<UIDebugOverlay, Void>(m => new UIDebugOverlayControl(m))
                .Add<VersionOverlay, Void>(m => new VersionOverlayControl(m))
                .ToDictionaries();
        }
    }
}
