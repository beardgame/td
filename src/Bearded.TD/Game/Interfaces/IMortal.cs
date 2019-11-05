using Bearded.TD.Game.Damage;
using Bearded.Utilities;

namespace Bearded.TD.Game
{
    interface IMortal
    {
        event GenericEventHandler<DamageInfo> Damaged;
        event GenericEventHandler<int> Healed;

        void OnDeath();
    }
}
