using Bearded.TD.Game.Damage;
using Bearded.Utilities;

namespace Bearded.TD.Game
{
    interface IMortal
    {
        void Damage(DamageInfo damageInfo);

        event GenericEventHandler<int> Healed;

        void OnDeath();
    }
}
