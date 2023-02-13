using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Shared.Events;
using JetBrains.Annotations;

namespace Bearded.TD.Game.GameLoop;

[GameRule("scheduleGame")]
sealed class ScheduleGame : GameRule<ScheduleGame.RuleParameters>
{
    public ScheduleGame(RuleParameters parameters) : base(parameters) {}

    public override void Execute(GameRuleContext context)
    {
        context.Dispatcher.RunOnlyOnServer(commandDispatcher =>
        {
            var (targetFaction, chaptersPerGame, wavesPerChapter, elements, enemies) = Parameters;
            var waveScheduler = new WaveScheduler(
                context.GameState,
                context.Factions.Find(targetFaction),
                enemies.CastArray<ISpawnableEnemy>(),
                commandDispatcher,
                context.Seed,
                context.Logger);
            var chapterScheduler = new ChapterScheduler(waveScheduler, elements, context.Seed, context.Logger);
            var gameScheduler = new GameScheduler(
                context.GameState,
                commandDispatcher,
                chapterScheduler,
                new GameScheduler.GameRequirements(chaptersPerGame, wavesPerChapter));
            context.Events.Subscribe(new Listener(gameScheduler));
        });
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed record RuleParameters(
        ExternalId<Faction> TargetFaction,
        int ChaptersPerGame,
        int WavesPerChapter,
        ImmutableArray<Element> Elements,
        ImmutableArray<SpawnableEnemy> Enemies);

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed record SpawnableEnemy(IGameObjectBlueprint Blueprint, double Probability) : ISpawnableEnemy;

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
