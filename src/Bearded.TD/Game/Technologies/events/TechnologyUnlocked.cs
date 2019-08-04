namespace Bearded.TD.Game.Technologies
{
    sealed class TechnologyUnlocked : IEvent
    {
        public ITechnologyBlueprint Technology { get; }

        public TechnologyUnlocked(ITechnologyBlueprint technology)
        {
            Technology = technology;
        }
    }
}
