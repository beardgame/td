using amulware.Graphics;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models
{
    class UnitBlueprint : IConvertsTo<Content.Models.UnitBlueprint, Void>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Health { get; set; }
        public int Damage { get; set; }
        public TimeSpan TimeBetweenAttacks { get; set; }
        public Speed Speed { get; set; }
        public float Value { get; set; }
        public Color Color { get; set; }

        public Content.Models.UnitBlueprint ToGameModel(Void _)
        {
            return new Content.Models.UnitBlueprint(Id, Name, Health, Damage, TimeBetweenAttacks, Speed, Value, Color);
        }
    }
}
