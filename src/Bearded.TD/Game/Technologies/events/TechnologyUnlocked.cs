namespace Bearded.TD.Game.Technologies
{
    sealed class TechnologyUnlocked : IEvent
    {
        public Technology Technology { get; }

        public TechnologyUnlocked(Technology technology)
        {
            Technology = technology;
        }
    }
}
