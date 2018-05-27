using System;
using System.Collections.Generic;
using Bearded.UI.Navigation;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Controls
{
    static class UILibrary
    {
        public static (IDictionary<Type, object> models, IDictionary<Type, object> views) CreateFactories()
        {
            return NavigationFactories.ForBoth()
                .Add<MainMenu, Void>(() => new MainMenu(), m => new MainMenuView(m))
                .Add<Lobby, LobbyManager>(() => new Lobby(), m => new LobbyView(m))
                .ToDictionaries();
        }
    }
}
