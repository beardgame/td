using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Utilities;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using OpenTK.Input;

namespace Bearded.TD.UI.Controls
{
    sealed class LoadingScreen : UpdateableNavigationNode<LoadingManager>
    {
        private LoadingManager loadingManager;
        private ShortcutManager shortcutManager;

        private readonly Dictionary<Key, Id<ShortcutManager.Shortcut>> registeredShortcuts =
            new Dictionary<Key, Id<ShortcutManager.Shortcut>>();

        public event VoidEventHandler ModLoadingUpdated;

        public IReadOnlyList<ModLoadingProfiler.BlueprintLoadingProfile> LoadedBlueprints =>
            loadingManager.LoadedBlueprints;

        public IReadOnlyList<string> LoadingBlueprints => loadingManager.LoadingBlueprints;

        protected override void Initialize(DependencyResolver dependencies, LoadingManager loadingManager)
        {
            base.Initialize(dependencies, loadingManager);

            shortcutManager = dependencies.Resolve<ShortcutManager>();
            registeredShortcuts.Add(Key.Space, shortcutManager.RegisterShortcut(Key.Space, loadingManager.Unpause));
            registeredShortcuts.Add(Key.Enter, shortcutManager.RegisterShortcut(Key.Enter, loadingManager.Unpause));

            this.loadingManager = loadingManager;
            loadingManager.Game.GameStatusChanged += onGameStatusChanged;
        }

        public override void Terminate()
        {
            base.Terminate();

            loadingManager.Game.GameStatusChanged -= onGameStatusChanged;
            foreach (var (key, id) in registeredShortcuts)
            {
                shortcutManager.RemoveShortcut(key, id);
            }
        }

        public override void Update(UpdateEventArgs args)
        {
            loadingManager.Update(args);
            ModLoadingUpdated?.Invoke();
        }

        private void onGameStatusChanged(GameStatus gameStatus)
        {
            if (gameStatus != GameStatus.Playing) throw new Exception("Unexpected game status change.");
            startGame();
        }

        private void startGame()
        {
            loadingManager.PrepareUI();
            Navigation.Replace<GameUI, (GameInstance, GameRunner)>(
                (loadingManager.Game, new GameRunner(loadingManager.Game, loadingManager.Network)), this);
            loadingManager.FinalizeUI();
        }

        public void Unpause()
        {
            loadingManager.Unpause();
        }
    }
}
