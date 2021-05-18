namespace Bearded.TD.Game.Simulation.Rules
{
    // ReSharper disable once UnusedTypeParameter
    interface IGameRule<in TOwner>
    {
        void Execute(GameRuleContext context);
    }
}
