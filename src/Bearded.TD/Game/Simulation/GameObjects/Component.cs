﻿using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameObjects;

abstract class Component<TParameters> : IComponent
    where TParameters : IParametersTemplate<TParameters>
{
    public ImmutableArray<string> Keys { get; init; } = ImmutableArray<string>.Empty;

    protected TParameters Parameters { get; }

    protected GameObject Owner { get; private set; } = null!;

    protected ComponentEvents Events { get; private set; } = null!;

    protected Component(TParameters parameters)
    {
        Parameters = parameters.CreateModifiableInstance();
    }

    public void OnAdded(GameObject owner, ComponentEvents events)
    {
        Owner = owner;
        Events = events;
        OnAdded();
    }

    protected abstract void OnAdded();

    public virtual void Activate() {}

    public virtual void OnRemoved() {}

    public abstract void Update(TimeSpan elapsedTime);

    public virtual void PreviewUpgrade(IUpgradePreview upgradePreview)
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
        OnAdded();
    }

    protected abstract void OnAdded();

    public virtual void Activate() {}

    public virtual void OnRemoved() {}

    public abstract void Update(TimeSpan elapsedTime);

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
