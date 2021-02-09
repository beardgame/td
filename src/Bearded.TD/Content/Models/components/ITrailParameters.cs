using amulware.Graphics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface ITrailParameters : IParametersTemplate<ITrailParameters>
    {
        bool SurviveObjectDeletion { get; }

        Unit Width { get; }

        TimeSpan Timeout { get; }

        ISpriteBlueprint Sprite { get; }

        Color Color { get; }
    }
}
