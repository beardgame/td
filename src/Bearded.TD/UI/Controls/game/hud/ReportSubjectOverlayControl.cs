using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class ReportSubjectOverlayControl : CompositeControl
    {
        private readonly ReportSubjectOverlay model;

        private readonly Binding<string> name = new();
        private readonly Binding<string> ownerName = new();
        private readonly Binding<Color> ownerColor = new();
        private readonly Binding<IReportSubject> reports = new();

        public ReportSubjectOverlayControl(ReportSubjectOverlay model)
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

            onReportSubjectSet();
        }

        private void onReportSubjectSet()
        {
            updateAttributes();
            reports.SetFromSource(model.Subject);
        }

        private void updateAttributes()
        {
            name.SetFromSource(model.Subject.Name);
            ownerName.SetFromSource(model.Subject.Faction?.Name ?? "");
            ownerColor.SetFromSource(model.Subject.Faction?.Color ?? Color.Black);
        }
    }
}
