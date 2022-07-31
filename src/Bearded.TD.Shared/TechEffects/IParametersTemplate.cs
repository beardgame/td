using Bearded.Utilities;

namespace Bearded.TD.Shared.TechEffects;

public interface IParametersTemplate
{
    bool HasAttributeOfType(AttributeType type);
    bool AddModification(AttributeType type, Modification modification);
    bool AddModificationWithId(AttributeType type, ModificationWithId modification);
    bool UpdateModification(AttributeType type, Id<Modification> id, Modification modification);
    bool RemoveModification(AttributeType type, Id<Modification> id);
}

public interface IParametersTemplate<out T> : IParametersTemplate where T : IParametersTemplate<T>
{
    T CreateModifiableInstance();
}
