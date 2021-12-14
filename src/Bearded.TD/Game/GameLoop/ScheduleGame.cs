using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.GameLoop;

[GameRule("scheduleGame")]
sealed class ScheduleGame : GameRule<ScheduleGame.RuleParameters>
{
    public ScheduleGame(RuleParameters parameters) : base(parameters) {}

    public override void Execute(GameRuleContext context)
    {
        context.Dispatcher.RunOnlyOnServer(commandDispatcher =>
        {
            var waveScheduler = new WaveScheduler(context.GameState, commandDispatcher);
            var chapterScheduler = new ChapterScheduler(waveScheduler);
            var (chaptersPerGame, wavesPerChapter) = Parameters;
            var gameScheduler = new GameScheduler(
                context.GameState,
                commandDispatcher,
                chapterScheduler,
                new GameScheduler.GameRequirements(chaptersPerGame, wavesPerChapter));
            context.Events.Subscribe(new Listener(gameScheduler));
        });
    }

    public sealed record RuleParameters(int ChaptersPerGame, int WavesPerChapter);

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