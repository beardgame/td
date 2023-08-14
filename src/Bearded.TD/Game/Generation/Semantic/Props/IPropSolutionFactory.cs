using System;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Props;

interface IPropSolutionFactory
{
    SolutionAction MakeSolution(Tile tile, Random random);
}
