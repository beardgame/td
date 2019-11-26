namespace Bearded.TD.Game.Technologies
{
    struct TechnologyDequeued : IEvent
    {
        public ITechnologyBlueprint Technology { get; }

        public TechnologyDequeued(ITechnologyBlueprint technology)
        {
            Technology = technology;
        }
    }
}
