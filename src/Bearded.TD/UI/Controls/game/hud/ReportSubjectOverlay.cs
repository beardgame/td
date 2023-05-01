using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.UI.Shortcuts;
using Bearded.TD.Utilities;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using JetBrains.Annotations;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Controls;

[UsedImplicitly]
sealed class ReportSubjectOverlay : UpdateableNavigationNode<IReportSubject>
{
    private GameInstance? game;
    private ShortcutCapturer? shortcutCapturer;

    private Pulse? pulse;
    private ShortcutLayer? shortcutLayer;

    public IReportSubject Subject { get; private set; } = null!;

    public GameInstance Game => game!;
    public IPulse Pulse => pulse!;

    public VoidEventHandler? Closing;

    protected override void Initialize(DependencyResolver dependencies, IReportSubject subject)
    {
        base.Initialize(dependencies, subject);
        Subject = subject;

        game = dependencies.Resolve<GameInstance>();
        shortcutCapturer = dependencies.Resolve<ShortcutCapturer>();

        pulse = new Pulse(game!.State.GameTime, Constants.UI.Statistics.TimeBetweenUIUpdates);
        shortcutLayer = ShortcutLayer.CreateBuilder()
            .AddShortcut(Keys.Escape, Close)
            .Build();
        shortcutCapturer!.AddLayer(shortcutLayer);
    }

    public override void Terminate()
    {
        if (shortcutLayer is not null)
        {
            shortcutCapturer?.RemoveLayer(shortcutLayer);
        }
        Closing?.Invoke();
        base.Terminate();
    }

    public override void Update(UpdateEventArgs args)
    {
        pulse?.Update();
    }

    public void Close()
    {
        Navigation?.Exit();
    }
}
