using Bearded.TD.Meta;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class DebugInvulnerable : DamageModifier
{
    protected override DamageShell AffectedShell => DamageShell.Health;

    public override void ModifyDamage(ref DamagePreview preview)
    {
        if (UserSettings.Instance.Debug.InvulnerableBuildings)
        {
            preview.Resist(Resistance.Full);
        }
    }
}
