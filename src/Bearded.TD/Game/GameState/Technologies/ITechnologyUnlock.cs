namespace Bearded.TD.Game.GameState.Technologies
{
    interface ITechnologyUnlock
    {
        string Description { get; }

        void Apply(TechnologyManager technologyManager);
    }
}
