namespace Bearded.TD.Game.Simulation.Rules;

abstract class GameRule<TParameters> : IGameRule
{
    protected TParameters Parameters { get; }

    protected GameRule(TParameters parameters)
    {
        Parameters = parameters;
    }

    public abstract void Execute(GameRuleContext context);
}

abstract class GameRule : GameRule<VoidParameters>
{
    protected GameRule() : base(null) {}
}
