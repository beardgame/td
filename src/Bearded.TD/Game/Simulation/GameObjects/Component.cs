using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameObjects;

abstract class Component<TParameters> : Component
    where TParameters : IParametersTemplate<TParameters>
{
    protected TParameters Parameters { get; }

    protected Component(TParameters parameters)
    {
        Parameters = parameters.CreateModifiableInstance();
    }

    public override void PreviewUpgrade(IUpgradePreview upgradePreview)
    {
        upgradePreview.RegisterParameters(Owner, Parameters);
    }
}

abstract class Component : IComponent
{
    public ImmutableArray<string> Keys { get; init; } = ImmutableArray<string>.Empty;

    protected GameObject Owner { get; private set; } = null!;

    protected ComponentEvents Events { get; private set; } = null!;

    public void OnAdded(GameObject owner, ComponentEvents events)
    {
        Owner = owner;
        Events = events;
        RegisterHandlers();
        OnAdded();
    }

    protected virtual void RegisterHandlers() {}

    protected virtual void OnAdded() {}

    public virtual void Activate() {}

    public void OnRemoved()
    {
        UnregisterHandlers();
        OnRemovedInternal();
    }

    protected virtual void UnregisterHandlers() {}

    protected virtual void OnRemovedInternal() {}

    public virtual void Update(TimeSpan elapsedTime) {}

    public virtual void PreviewUpgrade(IUpgradePreview upgradePreview) {}

    public virtual bool CanApplyUpgradeEffect(IUpgradeEffect effect) => false;

    public virtual void ApplyUpgradeEffect(IUpgradeEffect effect) { }

    public virtual bool RemoveUpgradeEffect(IUpgradeEffect effect) => false;
}

sealed class VoidParameters : IParametersTemplate<VoidParameters>
{
    public static VoidParameters Instance => new();

    private VoidParameters() { }

    public VoidParameters CreateModifiableInstance() => this;

    public bool HasAttributeOfType(AttributeType type) => false;

    public bool AddModification(AttributeType type, Modification modification) => false;
    public bool AddModificationWithId(AttributeType type, ModificationWithId modification) => false;
    public bool UpdateModification(AttributeType type, Id<Modification> id, Modification modification) => false;
    public bool RemoveModification(AttributeType type, Id<Modification> id) => false;
}
