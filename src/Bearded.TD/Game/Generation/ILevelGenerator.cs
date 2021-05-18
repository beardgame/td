using System.Collections.Generic;
using Bearded.TD.Game.Generation.Semantic.Commands;

namespace Bearded.TD.Game.Generation
{
    interface ILevelGenerator
    {
        IEnumerable<CommandFactory> Generate(LevelGenerationParameters parameters, int seed);
    }
}
