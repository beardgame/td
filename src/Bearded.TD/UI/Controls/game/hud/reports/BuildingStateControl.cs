using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class BuildingStateControl : ReportControl
{
    private readonly GameInstance game;
    private readonly IBuildingStateReport report;
    private readonly Binding<bool> buttonEnabled = new();
    private readonly Binding<string> buttonLabel = new();

    public override double Height { get; }

    public BuildingStateControl(GameInstance game, IBuildingStateReport report)
    {
        this.game = game;
        this.report = report;

        var column = this.BuildFixedColumn();
        column
            .AddButton(b => b
                .WithLabel(buttonLabel)
                .WithOnClick(deleteBuilding)
                .WithEnabled(buttonEnabled));
        Height = column.Height;

        Update();
    }

    private void deleteBuilding()
    {
        game.Request(DeleteBuilding.Request, report.Building);
    }

    public override void Update()
    {
        if (report.CanBeDeleted != buttonEnabled.Value)
        {
            buttonEnabled.SetFromSource(report.CanBeDeleted);
        }
        buttonLabel.SetFromSource(report.IsMaterialized ? $"Delete (refund: {report.RefundValue})" : "Cancel");
    }

    public override void Dispose() {}
}
