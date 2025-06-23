namespace Bearded.TD.Game.Simulation.Rules;

abstract class GameRule<TParameters>(TParameters parameters) : IGameRule
{
    protected TParameters Parameters { get; } = parameters;

    public abstract void Execute(GameRuleContext context);
}

abstract class GameRule() : GameRule<VoidParameters>(null!);
