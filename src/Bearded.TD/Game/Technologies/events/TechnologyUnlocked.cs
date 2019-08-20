namespace Bearded.TD.Game.Technologies
{
    struct TechnologyUnlocked : IEvent
    {
        public ITechnologyBlueprint Technology { get; }

        public TechnologyUnlocked(ITechnologyBlueprint technology)
        {
            Technology = technology;
        }
    }
}
