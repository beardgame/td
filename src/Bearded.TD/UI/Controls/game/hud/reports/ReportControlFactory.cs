using System;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed partial class ReportControlFactory : IReportControlFactory
{
    private readonly GameInstance game;
    private readonly IPulse pulse;

    public ReportControlFactory(GameInstance game, IPulse pulse)
    {
        this.game = game;
        this.pulse = pulse;
    }

    public Control CreateForReport(
        IReport report, Disposer disposer, ControlContainer detailsContainer, out double height)
    {
        var control = createForReport(report, detailsContainer);
        height = control.Height;
        pulse.Heartbeat += control.Update;
        disposer.AddDisposable(new ReportDisposer(control, pulse));
        return control;
    }

    private sealed class ReportDisposer : IDisposable
    {
        private readonly ReportControl control;
        private readonly IPulse pulse;

        public ReportDisposer(ReportControl control, IPulse pulse)
        {
            this.control = control;
            this.pulse = pulse;
        }

        public void Dispose()
        {
            control.Dispose();
            pulse.Heartbeat -= control.Update;
        }
    }
}
