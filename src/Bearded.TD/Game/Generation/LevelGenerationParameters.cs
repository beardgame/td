using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Generation;

sealed record LevelGenerationParameters(int Radius, NodeGroup Nodes, IBiome Biome);
