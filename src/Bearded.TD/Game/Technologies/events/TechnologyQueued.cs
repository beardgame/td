namespace Bearded.TD.Game.Technologies
{
    struct TechnologyQueued : IEvent
    {
        public ITechnologyBlueprint Technology { get; }

        public TechnologyQueued(ITechnologyBlueprint technology)
        {
            Technology = technology;
        }
    }
}
