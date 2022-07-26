using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

readonly record struct PreviewDelayNextShot(TimeSpan Delay) : IComponentPreviewEvent;
