using System.Collections.ObjectModel;
using Bearded.Graphics.Textures;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;
using SixLabors.ImageSharp;

namespace Bearded.TD.Content.Models;

sealed class Material : IBlueprint
{
    public ModAwareId Id { get; }
    public Shader Shader { get; }
    public ReadOnlyCollection<(string UniformName, Image Texture)> Textures { get; }

    public Material(ModAwareId id, Shader shader, ReadOnlyCollection<(string, Image)> textures)
    {
        Id = id;
        Shader = shader;
        Textures = textures;
    }
}
