namespace Bearded.TD.Game.Buildings
{
    struct Blueprint
    {
        public Footprint Footprint { get; }
        public int MaxHealth { get; }

        public Blueprint(Footprint footprint, int maxHealth)
        {
            Footprint = footprint;
            MaxHealth = maxHealth;
        }
    }
}
