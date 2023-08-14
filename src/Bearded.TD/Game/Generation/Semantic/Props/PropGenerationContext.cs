using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Props;

sealed class PropGenerationContext
{
    private readonly Tilemap<TileGeometry> geometry;
    private readonly Tilemap<IBiome> biomes;
    private readonly ImmutableArray<PropHint> propHints;
    private readonly PropSolver propSolver;

    public Random Random { get; }

    public PropGenerationContext(
        Tilemap<TileGeometry> geometry,
        Tilemap<IBiome> biomes,
        IEnumerable<PropHint> propHints,
        PropSolver propSolver,
        Random random)
    {
        this.geometry = geometry;
        this.biomes = biomes;
        this.propHints = propHints.ToImmutableArray();
        this.propSolver = propSolver;
        Random = random;
    }

    public TileGeometry GeometryFor(Tile tile) => geometry[tile];
    public IBiome BiomeFor(Tile tile) => biomes[tile];

    public IEnumerable<PropHint> EnumerateHints(PropPurpose purpose) => propHints.Where(h => h.Purpose == purpose);

    public void ProposeSolution(PropHint hint, SolutionAction solutionAction)
    {
        propSolver.AddOption(hint, solutionAction);
    }
}
