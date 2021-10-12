using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class RuinedReportControl : ReportControl
    {
        private readonly GameInstance game;
        private readonly IRuinedReport report;
        private readonly Binding<bool> buttonEnabled = new();
        private readonly Binding<double> buttonProgress = new();

        public override double Height { get; }

        public RuinedReportControl(GameInstance game, IRuinedReport report)
        {
            this.game = game;
            this.report = report;

            var column = this.BuildFixedColumn();
            column
                .AddButton(b => b
                    .WithLabel(label)
                    .WithOnClick(startRepair)
                    .WithResourceCost(report.RepairCost)
                    .WithEnabled(buttonEnabled)
                    .WithProgressBar(buttonProgress));
            Height = column.Height;

            Update();
        }

        private string label()
        {
            if (!report.RepairInProgress)
            {
                return "Repair";
            }

            return report.PercentageComplete > 0 ? "Repair (in progress)" : "Repair (queued)";
        }

        private void startRepair()
        {
            game.Request(RepairBuilding.Request, game.Me.Faction, report.Building);
        }

        public override void Update()
        {
            var canBeRepaired = report.CanBeRepairedBy(game.Me.Faction);
            if (canBeRepaired != buttonEnabled.Value)
            {
                buttonEnabled.SetFromSource(canBeRepaired);
            }

            var repairProgress = report.PercentageComplete;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (repairProgress != buttonProgress.Value)
            {
                buttonProgress.SetFromSource(repairProgress);
            }
        }

        public override void Dispose() {}
    }
}
