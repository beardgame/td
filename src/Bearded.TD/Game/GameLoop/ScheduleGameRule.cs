using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;

namespace Bearded.TD.Game.GameLoop
{
    [GameRule("scheduleGame")]
    sealed class ScheduleGameRule : GameRule<ScheduleGameRule.Parameters>
    {
        public ScheduleGameRule(Parameters parameters) : base(parameters) {}

        protected override void Execute(GameState gameState, Parameters parameters)
        {
            gameState.Meta.Dispatcher.RunOnlyOnServer(commandDispatcher =>
            {
                var waveScheduler = new WaveScheduler(gameState, commandDispatcher);
                var chapterScheduler = new ChapterScheduler(waveScheduler);
                var (chaptersPerGame, wavesPerChapter) = parameters;
                var gameScheduler = new GameScheduler(
                    gameState,
                    commandDispatcher,
                    chapterScheduler,
                    new GameScheduler.GameRequirements(chaptersPerGame, wavesPerChapter));
                gameState.Meta.Events.Subscribe(new Listener(gameScheduler));
            });
        }

        public sealed record Parameters(int ChaptersPerGame, int WavesPerChapter);

        private sealed class Listener : IListener<GameStarted>
        {
            private readonly GameScheduler gameScheduler;

            public Listener(GameScheduler gameScheduler)
            {
                this.gameScheduler = gameScheduler;
            }

            public void HandleEvent(GameStarted @event)
            {
                gameScheduler.StartGame();
            }
        }
    }
}
