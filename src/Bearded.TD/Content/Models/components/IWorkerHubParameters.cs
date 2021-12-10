using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Models;

interface IWorkerHubParameters : IParametersTemplate<IWorkerHubParameters>
{
    [Modifiable(Type = AttributeType.DroneCount)] int NumWorkers { get; }

    IComponentOwnerBlueprint Drone { get; }
}