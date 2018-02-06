namespace Bearded.TD.Game.Units.StatusEffects
{
    interface IUnitStatusEffect
    {
        void Apply(EnemyUnitProperties.Builder propertiesBuilder);
    }
}
