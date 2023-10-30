﻿using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Upgrades;

abstract class UpgradeEffectBase : IUpgradeEffect
{
    public UpgradePrerequisites Prerequisites { get; }
    public bool IsSideEffect { get; }

    protected UpgradeEffectBase(UpgradePrerequisites prerequisites, bool isSideEffect)
    {
        Prerequisites = prerequisites;
        IsSideEffect = isSideEffect;
    }

    public virtual bool ModifiesComponentCollection => false;

    public virtual ComponentTransaction CreateComponentChanges(GameObject gameObject) =>
        ComponentTransaction.Empty(gameObject);

    public virtual bool CanApplyTo(IParametersTemplate subject) => false;

    public virtual void ApplyTo(IParametersTemplate subject) {}

    public virtual bool RemoveFrom(IParametersTemplate subject) => false;
}
