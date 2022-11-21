using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

readonly record struct TargetingContext(Position3 WeaponPosition, Direction2? CurrentAimDirection);
