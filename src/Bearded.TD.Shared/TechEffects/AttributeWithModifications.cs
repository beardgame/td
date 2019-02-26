using System;
using System.Collections.Generic;
using Bearded.Utilities;

namespace Bearded.TD.Shared.TechEffects
{
    public sealed class AttributeWithModifications<T> : IAttributeWithModifications
    {
        private readonly double baseValue;
        private readonly Func<double, T> valueTransformer;
        private readonly List<ModificationWithId> additiveModifications = new List<ModificationWithId>();
        private readonly List<ModificationWithId> exponentialModifications = new List<ModificationWithId>();

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
            var val = baseValue;
            
            foreach (var mod in additiveModifications)
            {
                val = applyAdditiveModification(val, mod.Modification);
            }
            
            foreach (var mod in exponentialModifications)
            {
                val = applyExponentialModification(val, mod.Modification);
            }

            currentValue = valueTransformer(val);
            currentValueDirty = false;
        }

        private double applyAdditiveModification(double val, Modification modification)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (modification.Type)
            {
                case Modification.ModificationType.AdditiveAbsolute:
                    return val + modification.Value;
                case Modification.ModificationType.AdditiveRelative:
                    return val + modification.Value * baseValue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private double applyExponentialModification(double val, Modification modification)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (modification.Type)
            {
                case Modification.ModificationType.Exponent:
                    return val * modification.Value;
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
            switch (modification.Modification.Type)
            {
                case Modification.ModificationType.AdditiveAbsolute:
                case Modification.ModificationType.AdditiveRelative:
                    additiveModifications.Add(modification);
                    break;
                
                case Modification.ModificationType.Exponent:
                    exponentialModifications.Add(modification);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            currentValueDirty = true;
        }

        public bool RemoveModification(Id<Modification> id)
        {
            var deleted = additiveModifications.RemoveAll(m => m.Id == id) > 0;
            deleted |= exponentialModifications.RemoveAll(m => m.Id == id) > 0;
            currentValueDirty |= deleted;
            return deleted;
        }
    }
}
