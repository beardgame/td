using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models;

interface IWorkerParameters : IParametersTemplate<IWorkerParameters>
{
    [Modifiable(10, Type = AttributeType.MovementSpeed)]
    Speed MovementSpeed { get; }

    ResourceRate BuildingSpeed { get; }
}