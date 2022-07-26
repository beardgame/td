using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Shared.TechEffects;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public abstract class ModifiableBase<T> where T : ModifiableBase<T>
{
    private static IDictionary<AttributeType, List<Func<T, IAttributeWithModifications>>>? attributeGettersByType;

    protected static void InitializeAttributes(ILookup<AttributeType, Func<T, IAttributeWithModifications>> attributes)
    {
        if (attributeGettersByType != null)
            throw new InvalidOperationException("Do not initialise attributes multiple times.");

        attributeGettersByType = attributes
            .ToDictionary(
                g => g.Key,
                g => g.ToList()
            );
    }

    public virtual bool HasAttributeOfType(AttributeType type) => AttributeIsKnown(type);

    public virtual bool AddModification(AttributeType type, Modification modification)
        => addModificationOnInstance((T) this, type, modification);

    public virtual bool AddModificationWithId(AttributeType type, ModificationWithId modification)
        => addModificationWithIdOnInstance((T) this, type, modification);

    public virtual bool UpdateModification(AttributeType type, Id<Modification> id, Modification newModification)
        => updateModificationOnInstance((T) this, type, id, newModification);

    public virtual bool RemoveModification(AttributeType type, Id<Modification> id)
        => removeModificationOnInstance((T) this, type, id);

    public static bool AttributeIsKnown(AttributeType type) => attributeGettersByType!.ContainsKey(type);

    private static bool addModificationOnInstance(T instance, AttributeType type, Modification modification)
        => doOnInstance(instance, type, attr => attr.AddModification(modification));

    private static bool addModificationWithIdOnInstance(
        T instance,
        AttributeType type,
        ModificationWithId modification)
        => doOnInstance(instance, type, attr => attr.AddModificationWithId(modification));

    private static bool updateModificationOnInstance(
        T instance,
        AttributeType type,
        Id<Modification> id,
        Modification newModification)
        => doOnInstance(instance, type, attr => attr.UpdateModification(id, newModification));

    private static bool removeModificationOnInstance(T instance, AttributeType type, Id<Modification> id)
        => doOnInstance(instance, type, attr => attr.RemoveModification(id));

    private static bool doOnInstance(T instance, AttributeType type, Action<IAttributeWithModifications> action)
        => doOnInstance(instance, type, attr =>
        {
            action(attr);
            return true;
        });

    private static bool doOnInstance(T instance, AttributeType type, Func<IAttributeWithModifications, bool> action)
    {
        if (!attributeGettersByType!.TryGetValue(type, out var attributeGetters))
            return false;

        var result = false;

        foreach (var attributeGetter in attributeGetters)
        {
            var attribute = attributeGetter(instance);
            result |= action(attribute);
        }

        return result;
    }
}
