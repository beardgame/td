using Bearded.Utilities;

namespace Bearded.TD.Shared.TechEffects
{
    public struct ModificationWithId
    {
        public Id<Modification> Id { get; }
        public Modification Modification { get; }

        public ModificationWithId(Id<Modification> id, Modification modification)
        {
            Id = id;
            Modification = modification;
        }
    }
}
