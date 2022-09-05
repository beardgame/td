using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

readonly record struct PreviewMove(Position3 Start, Difference3 Step) : IComponentPreviewEvent;
