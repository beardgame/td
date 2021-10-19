using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Utilities;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.UI.Controls
{
    [UsedImplicitly]
    sealed class ReportSubjectOverlay : UpdateableNavigationNode<IReportSubject>
    {
        private GameInstance? game;
        private Pulse pulse = null!;

        public IReportSubject Subject { get; private set; } = null!;

        public GameInstance Game => game!;
        public IPulse Pulse => pulse;

        public VoidEventHandler? Closing;

        protected override void Initialize(DependencyResolver dependencies, IReportSubject subject)
        {
            base.Initialize(dependencies, subject);
            Subject = subject;

            game = dependencies.Resolve<GameInstance>();
            pulse = new Pulse(game.State, Constants.UI.Statistics.TimeBetweenUIUpdates);
        }


        public override void Terminate()
        {
            Closing?.Invoke();
            base.Terminate();
        }

        public override void Update(UpdateEventArgs args)
        {
            pulse.Update();
        }

        public void Close()
        {
            Navigation?.Exit();
        }
    }
}
