using Bearded.Utilities;

namespace Bearded.TD.Shared.TechEffects;

public interface IAttributeWithModifications
{
    void AddModification(Modification modification);
    void AddModificationWithId(ModificationWithId modification);
    void UpdateModification(Id<Modification> id, Modification newModification);
    bool RemoveModification(Id<Modification> id);
}
