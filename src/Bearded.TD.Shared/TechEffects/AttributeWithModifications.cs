using System;
using System.Collections.Generic;
using System.Linq;

namespace Bearded.TD.Shared.TechEffects
{
    sealed class AttributeWithModifications<T>
    {
        private readonly double baseValue;
        private readonly Func<double, T> valueTransformer;
        private readonly List<Modification> modifications = new List<Modification>();

        private bool currentValueDirty;
        private T currentValue;

        public T Value
        {
            get
            {
                if (currentValueDirty)
                {
                    recalculateCurrentValue();
                }
                return currentValue;
            }
        }

        public AttributeWithModifications(double baseValue, Func<double, T> valueTransformer)
        {
            this.baseValue = baseValue;
            this.valueTransformer = valueTransformer;
        }

        private void recalculateCurrentValue()
        {
            currentValue = valueTransformer(modifications.Aggregate(baseValue, applyModification));
            currentValueDirty = false;
        }

        private double applyModification(double val, Modification modification)
        {
            switch (modification.Type)
            {
                case Modification.ModificationType.Additive:
                    return val + modification.Value;
                case Modification.ModificationType.Multiplicative:
                    return val + modification.Value * baseValue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void AddModification(Modification modification)
        {
            modifications.Add(modification);
            currentValueDirty = true;
        }
    }
}
