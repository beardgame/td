using Bearded.Graphics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models;

enum ParticleDrawMode
{
    Sprite,
    Line
}

interface IParticleSystemParameters : IParametersTemplate<IParticleSystemParameters>
{
    int Count { get; }
    ISpriteBlueprint Sprite { get; }
    Color Color { get; }
    Shader? Shader { get; }
    float Size { get; }
    float LineWidth { get; }
    TimeSpan LifeTime { get; }
    Speed RandomVelocity { get; }
    Speed VectorVelocity { get; }
    ParticleDrawMode DrawMode { get; }
}