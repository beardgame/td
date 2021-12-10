using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Models;

interface ICost : IParametersTemplate<ICost>
{
    public ResourceAmount Resources { get; }
}