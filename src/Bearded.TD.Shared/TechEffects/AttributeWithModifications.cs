using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Utilities;

namespace Bearded.TD.Shared.TechEffects
{
    public sealed class AttributeWithModifications<T> : IAttributeWithModifications
    {
        private readonly double baseValue;
        private readonly Func<double, T> valueTransformer;
        private readonly List<ModificationWithId> modifications = new List<ModificationWithId>();

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
            currentValueDirty = true;
        }

        private void recalculateCurrentValue()
        {
            currentValue = valueTransformer(
                modifications.Select(m => m.Modification).Aggregate(baseValue, applyModification));
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

        public void UpdateModification(Id<Modification> id, Modification newModification)
        {
            if (!RemoveModification(id))
            {
                throw new InvalidOperationException("Cannot update a non-existent modification.");
            }
            AddModification(newModification);
        }

        public void AddModification(Modification modification) =>
            AddModificationWithId(new ModificationWithId(Id<Modification>.Invalid, modification));
        
        public void AddModificationWithId(ModificationWithId modification)
        {
            modifications.Add(modification);
            currentValueDirty = true;
        }
        
        public bool RemoveModification(Id<Modification> id)
        {
            var deleted = modifications.RemoveAll(m => m.Id == id) > 0;
            currentValueDirty |= deleted;
            return deleted;
        }
    }
}
