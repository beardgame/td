using Bearded.TD.Game;
using Bearded.TD.Game.Input;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class ManualControlReportControl : ReportControl
{
    private readonly GameInstance game;
    private readonly IManualControlReport report;

    private readonly Binding<bool> canControl = new();

    public override double Height { get; }

    public ManualControlReportControl(GameInstance game, IManualControlReport report)
    {
        this.game = game;
        this.report = report;

        var column = this.BuildFixedColumn();
        column.AddButton(b => b.WithLabel("Assume Direct Control").WithOnClick(startControl).WithEnabled(canControl));
        Height = column.Height;
    }

    private void startControl()
    {
        game.SelectionManager.ResetSelection();
        game.PlayerInput.SetInteractionHandler(new ManualControlInteractionHandler(game, report));
    }

    public override void Update()
    {
        canControl.SetFromSource(report.CanBeControlledBy(game.Me.Faction));
    }

    public override void Dispose()
    {
    }
}
