using Bearded.Utilities;

namespace Bearded.TD.Game
{
    interface IDamageable
    {
        int Health { get; }
        event VoidEventHandler Damaged;
        void Damage(int damage);
    }
}