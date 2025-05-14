using System.IO;

namespace Bearded.TD.Content.Mods;

sealed class ModDependency
{
    public string Id { get; }

    public ModDependency(Serialization.Models.ModDependency dependency)
    {
        Id = dependency.Id ?? throw new InvalidDataException();
    }
}
