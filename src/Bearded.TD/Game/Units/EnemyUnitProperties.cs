using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Units
{
    sealed class EnemyUnitProperties
    {
        public int Damage { get; }
        public TimeSpan TimeBetweenAttacks { get; }
        public Speed Speed { get; }

        private EnemyUnitProperties(int damage, TimeSpan timeBetweenAttacks, Speed speed)
        {
            Damage = damage;
            TimeBetweenAttacks = timeBetweenAttacks;
            Speed = speed;
        }

        public static Builder BuilderFromBlueprint(IUnitBlueprint blueprint)
        {
            return new Builder()
            {
                Damage = blueprint.Damage,
                TimeBetweenAttacks = blueprint.TimeBetweenAttacks,
                Speed = blueprint.Speed,
            };
        }

        public static EnemyUnitProperties FromState(EnemyUnitState state)
        {
            return new EnemyUnitProperties(state.Damage, state.TimeBetweenAttacks.S(), state.Speed.UnitsPerSecond());
        }

        public sealed class Builder
        {
            public int Damage { get; set; }
            public TimeSpan TimeBetweenAttacks { get; set; }
            public Speed Speed { get; set; }

            public EnemyUnitProperties Build()
            {
                return new EnemyUnitProperties(Damage, TimeBetweenAttacks, Speed);
            }
        }
    }
}
