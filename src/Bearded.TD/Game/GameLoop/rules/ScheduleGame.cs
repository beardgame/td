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
            var (targetFaction, chaptersPerGame, wavesPerChapter, enableTimers, elements, enemies) = Parameters;
            var spawnLocationActivator = new SpawnLocationActivator(context.GameState, commandDispatcher, context.Seed);
            var waveGenerator = new WaveGenerator(
                enemies.CastArray<ISpawnableEnemy>(),
                context.Blueprints.Modules.All,
                context.Factions.Find(targetFaction),
                context.Seed,
                context.Logger);
            var waveExecutor = new WaveExecutor(context.GameState, context.Ids, commandDispatcher);
            var chapterGenerator = new ChapterGenerator(elements, enableTimers, context.Seed);
            var chapterExecutor = new ChapterExecutor(spawnLocationActivator, waveGenerator, waveExecutor);
            var gameScheduler = new GameScheduler(
                context.GameState,
                commandDispatcher,
                chapterGenerator,
                chapterExecutor,
                new GameScheduler.GameRequirements(chaptersPerGame, wavesPerChapter));
            context.Events.Subscribe(new Listener(gameScheduler));
        });
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed record RuleParameters(
        ExternalId<Faction> TargetFaction,
        int ChaptersPerGame,
        int WavesPerChapter,
        bool EnableTimers,
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
