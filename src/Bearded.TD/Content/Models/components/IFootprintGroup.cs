using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Models;

interface IFootprintGroup : IParametersTemplate<IFootprintGroup>
{
    public FootprintGroup Group { get; }
}