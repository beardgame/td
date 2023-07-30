using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

readonly record struct TileGeometry(TileType Type, double Hardness, Unit FloorHeight);
