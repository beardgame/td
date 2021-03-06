﻿using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Upgrades
{
    abstract class UpgradeEffectBase : IUpgradeEffect
    {
        public bool CanApplyTo<T>(ComponentCollection<T> subject)
            => CanApplyToComponentCollectionForType<T>() || subject.Components.Any(c => c.CanApplyUpgradeEffect(this));

        public virtual bool CanApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T> => false;

        public virtual bool CanApplyToComponentCollectionForType<T>() => false;

        public virtual void ApplyTo<T>(ComponentCollection<T> subject)
            => subject.Components.ForEach(c => c.ApplyUpgradeEffect(this));

        public virtual void ApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T> {}

        protected virtual Maybe<IComponentFactory<T>> TryCreateComponentFactory<T>() => Maybe.Nothing;

        public virtual bool RemoveFrom<T>(ComponentCollection<T> subject)
            => subject.Components.Aggregate(false, (b, c) => c.RemoveUpgradeEffect(this) || b);

        public virtual bool RemoveFrom<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T> => false;
    }
}
