using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings;

readonly record struct BuildingConstructionFinished(GameObject GameObject) : IGlobalEvent;
