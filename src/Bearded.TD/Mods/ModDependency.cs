
namespace Bearded.TD.Mods
{
    sealed class ModDependency
    {
        public string Id { get; }
        public string Alias { get; }

        public ModDependency(Serialization.Models.ModDependency dependency)
        {
            Id = dependency.Id;
            Alias = dependency.Alias;
        }

    }
}
