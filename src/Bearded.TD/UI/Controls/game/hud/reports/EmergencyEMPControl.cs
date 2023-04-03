using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class EmergencyEMPControl : ReportControl
{
    private readonly GameInstance game;
    private readonly IEmergencyEMPReport report;

    private readonly Binding<bool> canControl = new();

    public override double Height { get; }

    public EmergencyEMPControl(GameInstance game, IEmergencyEMPReport report)
    {
        this.game = game;
        this.report = report;

        var column = this.BuildFixedColumn();
        column.AddButton(b => b.WithLabel("Emergency EMP").WithOnClick(onClick).WithEnabled(canControl));
        Height = column.Height;
    }

    private void onClick()
    {
        game.Request(FireEmergencyEMP.Request, report.Owner);
    }

    public override void Update()
    {
        canControl.SetFromSource(report.Available);
    }

    public override void Dispose()
    {
    }
}
