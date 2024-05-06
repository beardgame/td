using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;

namespace Bearded.TD.Content.Models.Fonts;

sealed class Font(ModAwareId id, FontDefinition definition, Material material)
    : IBlueprint
{
    public ModAwareId Id { get; } = id;
    public FontDefinition Definition { get; } = definition;
    public Material Material { get; } = material;
}
