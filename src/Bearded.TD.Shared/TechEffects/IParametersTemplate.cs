using Bearded.Utilities;

namespace Bearded.TD.Shared.TechEffects
{
    public interface IParametersTemplate<out T> where T : IParametersTemplate<T>
    {
        bool HasAttributeOfType(AttributeType type);
        bool AddModification(AttributeType type, Modification modification);
        bool AddModificationWithId(AttributeType type, ModificationWithId modification);
        bool UpdateModification(AttributeType type, Id<Modification> id, Modification modification);
        bool RemoveModification(AttributeType type, Id<Modification> id);
        T CreateModifiableInstance();
    }
}
