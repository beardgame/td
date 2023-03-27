using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class ManualOverdriveReportControl : ReportControl
{
    private readonly GameInstance game;
    private readonly IManualOverdriveReport report;

    private readonly Binding<bool> canControl = new();

    public override double Height { get; }

    public ManualOverdriveReportControl(GameInstance game, IManualOverdriveReport report)
    {
        this.game = game;
        this.report = report;

        var column = this.BuildFixedColumn();
        column.AddButton(b => b.WithLabel("Enable overdrive").WithOnClick(startControl).WithEnabled(canControl));
        Height = column.Height;
    }

    private void startControl()
    {
        report.StartControl(report.EndControl);
    }

    public override void Update()
    {
        canControl.SetFromSource(report.CanBeControlledBy(game.Me.Faction));
    }

    public override void Dispose()
    {
    }
}
