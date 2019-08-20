namespace Bearded.TD.Game.Technologies
{
    sealed class TechnologyQueued : IEvent
    {
        public ITechnologyBlueprint Technology { get; }

        public TechnologyQueued(ITechnologyBlueprint technology)
        {
            Technology = technology;
        }
    }
}
