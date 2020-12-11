using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Damage
{
    interface IMortal
    {
        void Damage(DamageInfo damageInfo);

        event GenericEventHandler<int>? Healed;

        void OnDeath();
    }
}
