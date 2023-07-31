using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Generation;

sealed record LevelGenerationParameters(int Radius, NodeGroup Nodes, IEnumerable<IBiome> Biomes);
