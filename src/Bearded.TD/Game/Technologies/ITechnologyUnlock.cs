namespace Bearded.TD.Game.Technologies
{
    interface ITechnologyUnlock
    {
        string Description { get; }

        void Apply(TechnologyManager technologyManager);
    }
}
