using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.UI;
using Bearded.Utilities.Linq;
using JetBrains.Annotations;

namespace Bearded.TD.Game.GameLoop;

[GameRule("destroyBuildingsAtChapterEnd")]
sealed class DestroyBuildingsAtChapterEnd(DestroyBuildingsAtChapterEnd.RuleParameters parameters)
    : GameRule<DestroyBuildingsAtChapterEnd.RuleParameters>(parameters)
{
    public override void Execute(GameRuleContext context)
    {
        context.Events.Observe<WaveEnded>()
            .Where(w => w.Wave.Script.IsFinalWave)
            .Subscribe(_ => onChapterEnded(context.GameState));
    }

    private void onChapterEnded(GameState gameState)
    {
        gameState.Meta.Dispatcher.RunOnlyOnServer(commandDispatcher =>
        {
            var destroyableBuildings = gameState.BuildingLayer
                .EnumerateAllObjects(gameState.Level)
                .Where(b => !b.GetComponents<GameOverOnDestroy>().Any()) // We don't want the player to randomly game over!
                .ToImmutableArray();
            var buildingsToDestroyCount =
                Math.Clamp((int) (destroyableBuildings.Length * (Parameters.Fraction ?? 0.5)), 1, destroyableBuildings.Length);
            var buildingsToDestroy = destroyableBuildings.RandomSubset(buildingsToDestroyCount);
            foreach (var building in buildingsToDestroy)
            {
                commandDispatcher.Dispatch(KillGameObject.Command(building, null));
            }
        });
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed record RuleParameters(double? Fraction);
}
