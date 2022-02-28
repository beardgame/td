using System;
using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Networking;
using Bearded.UI.Navigation;

namespace Bearded.TD.UI.Controls;

sealed class LoadingScreen : UpdateableNavigationNode<LoadingManager>
{
    private LoadingManager loadingManager = null!;

    protected override void Initialize(DependencyResolver dependencies, LoadingManager loadingManager)
    {
        base.Initialize(dependencies, loadingManager);

        this.loadingManager = loadingManager;
        loadingManager.Game.GameStatusChanged += onGameStatusChanged;
    }

    public override void Terminate()
    {
        base.Terminate();

        loadingManager.Game.GameStatusChanged -= onGameStatusChanged;
    }

    public override void Update(UpdateEventArgs args)
    {
        loadingManager.Update(args);
    }

    private void onGameStatusChanged(GameStatus gameStatus)
    {
        if (gameStatus != GameStatus.Playing) throw new Exception("Unexpected game status change.");
        startGame();
    }

    private void startGame()
    {
        loadingManager.PrepareUI();
        Navigation!.Replace<GameUI, (GameInstance, NetworkInterface)>(
            (loadingManager.Game, loadingManager.Network), this);
        loadingManager.FinalizeUI();
    }
}
