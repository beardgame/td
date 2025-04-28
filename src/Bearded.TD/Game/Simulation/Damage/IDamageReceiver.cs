namespace Bearded.TD.Game.Simulation.Damage;

interface IDamageReceiver
{
    DamageShell Shell { get; }
    IntermediateDamageResult ApplyDamage(TypedDamage damage, Hit hit, IDamageSource? source);
}
