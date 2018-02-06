using System;

namespace Bearded.TD.Game.Units.StatusEffects
{
    static class UnitStatusEffects
    {
        public static IUnitStatusEffect Slow = new SimpleStatusEffect(properties =>
        {
            properties.Speed *= .9f;
            properties.TimeBetweenAttacks *= 1.11;
        }, 3);

        public static IUnitStatusEffect Weakened = new SimpleStatusEffect(properties => properties.Damage /= 2, 1);
        
        private class SimpleStatusEffect : IUnitStatusEffect
        {
            private readonly Action<EnemyUnitProperties.Builder> effect;

            public int MaxStackSize { get; }

            public SimpleStatusEffect(Action<EnemyUnitProperties.Builder> effect, int maxStackSize)
            {
                this.effect = effect;
                MaxStackSize = maxStackSize;
            }

            public void Apply(EnemyUnitProperties.Builder propertiesBuilder) => effect(propertiesBuilder);
        }
    }
}
