namespace Bearded.TD.Game.Technologies
{
    interface ITechnologyEffect
    {
        string Description { get; }

        void Unlock(TechnologyManager technologyManager);
    }
}
