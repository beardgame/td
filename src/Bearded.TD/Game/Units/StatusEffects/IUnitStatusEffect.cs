namespace Bearded.TD.Game.Units.StatusEffects
{
    interface IUnitStatusEffect
    {
        int MaxStackSize { get; }

        void Apply(EnemyUnitProperties.Builder propertiesBuilder);
    }
}
