using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Damage;

abstract class DamageModifier : Component, IDamageModifier
{
    private DamageModifiers.ModifierRemover? modifierRemover;

    protected abstract DamageShell AffectedShell { get; }

    protected override void OnAdded()
    {
        DamageModifiers.ModifyShell(AffectedShell, this, Owner, Events, out modifierRemover);
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        modifierRemover?.Invoke();
        modifierRemover = null;
    }

    public override void Update(TimeSpan elapsedTime) { }

    public abstract void ModifyDamage(ref DamagePreview preview);
}

abstract class DamageModifier<T>(T parameters)
    : Component<T>(parameters), IDamageModifier where T : IParametersTemplate<T>
{
    private DamageModifiers.ModifierRemover? modifierRemover;

    protected abstract DamageShell AffectedShell { get; }

    protected override void OnAdded()
    {
        DamageModifiers.ModifyShell(AffectedShell, this, Owner, Events, out modifierRemover);
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        modifierRemover?.Invoke();
        modifierRemover = null;
    }

    public override void Update(TimeSpan elapsedTime) { }

    public abstract void ModifyDamage(ref DamagePreview preview);
}
