using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class BuildingStatusOverlayControl : CompositeControl
    {
        private readonly BuildingStatusOverlay model;

        private readonly Binding<string> name = new();
        private readonly Binding<string> ownerName = new();
        private readonly Binding<Color> ownerColor = new();
        private readonly Binding<IReportSubject> reports = new();

        public BuildingStatusOverlayControl(BuildingStatusOverlay model)
        {
            IsClickThrough = true;

            this.model = model;
            this.BuildLayout()
                .ForFullScreen()
                .AddEntityStatus(b => b
                    .WithName(name)
                    .AddTextAttribute("Owned by", ownerName, ownerColor)
                    .WithReports(reports, new ReportControlFactory(model.Game, model.Pulse))
                    .WithCloseAction(model.Close));

            onBuildingSet();
        }

        private void onBuildingSet()
        {
            updateBuildingAttributes();
            reports.SetFromSource(model.Building as IReportSubject ?? new EmptyReportSubject());
        }

        private void updateBuildingAttributes()
        {
            name.SetFromSource(model.Building.Blueprint.Name);
            ownerName.SetFromSource(model.Building.Faction.Name);
            ownerColor.SetFromSource(model.Building.Faction.Color);
        }
    }
}
