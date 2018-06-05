using System;
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
                .Add<GameWorld, (GameInstance, GameRunner)>(
                    m => new GameWorldView(m, renderContext.Compositor, renderContext.Geometries))
                .Add<LoadingScreen, LoadingManager>(m => new LoadingScreenView(m))
                .Add<Lobby, LobbyManager>(m => new LobbyView(m))
                .Add<LobbyList, Void>(m => new LobbyListView(m))
                .Add<MainMenu, Void>(m => new MainMenuView(m))
                .ToDictionaries();
        }
    }
}
