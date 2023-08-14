namespace Bearded.TD.Game.Generation.Semantic.Props;

sealed class PropRule : IPropRule
{
    private readonly PropRuleSelector selector;
    private readonly IPropSolutionFactory factory;

    public PropRule(PropRuleSelector selector, IPropSolutionFactory factory)
    {
        this.selector = selector;
        this.factory = factory;
    }

    public void Execute(PropGenerationContext context)
    {
        foreach (var hint in context.EnumerateHints(selector.Purpose))
        {
            context.ProposeSolution(hint, factory.MakeSolution(hint.Tile, context.Random));
        }
    }
}
