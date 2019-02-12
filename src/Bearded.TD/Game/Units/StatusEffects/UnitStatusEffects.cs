using System;

namespace Bearded.TD.Game.Units.StatusEffects
{
    static class UnitStatusEffects
    {
        public static readonly IUnitStatusEffect Slow = new SimpleStatusEffectSingleton(properties =>
        {
            properties.Speed *= .9f;
        });
        
        private class SimpleStatusEffectSingleton : IUnitStatusEffect
        {
            private readonly Action<EnemyUnitProperties.Builder> effect;

            public SimpleStatusEffectSingleton(Action<EnemyUnitProperties.Builder> effect)
            {
                this.effect = effect;
            }
            
            public void Apply(EnemyUnitProperties.Builder propertiesBuilder) => effect(propertiesBuilder);
        }
    }
}
