using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Units
{
    sealed class EnemyUnitProperties
    {
        public Speed Speed { get; }

        private EnemyUnitProperties(Speed speed)
        {
            Speed = speed;
        }

        public static Builder BuilderFromBlueprint(IUnitBlueprint blueprint)
        {
            return new Builder()
            {
                Speed = blueprint.Speed,
            };
        }

        public static EnemyUnitProperties FromState(EnemyUnitState state)
        {
            return new EnemyUnitProperties(state.Speed.UnitsPerSecond());
        }

        public sealed class Builder
        {
            public Speed Speed { get; set; }

            public EnemyUnitProperties Build()
            {
                return new EnemyUnitProperties(Speed);
            }
        }
    }
}
