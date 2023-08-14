using System;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Generation.ObjectPositioning;

namespace Bearded.TD.Game.Generation.Semantic.Props;

sealed class GameObjectPropSolutionFactory : IPropSolutionFactory
{
    private readonly ImmutableArray<IGameObjectBlueprint> blueprints;
    private readonly AlignmentMode alignment;
    private readonly RotationMode rotation;
    private readonly Unit z;

    public GameObjectPropSolutionFactory(
        ImmutableArray<IGameObjectBlueprint> blueprints, AlignmentMode alignment, RotationMode rotation, Unit z)
    {
        this.rotation = rotation;
        this.z = z;
        this.alignment = alignment;
        this.blueprints = blueprints;
    }

    public SolutionAction MakeSolution(Tile tile, Random random)
    {
        var blueprint = blueprints.RandomElement(random);
        var position = alignment.ToPosition(tile, random).WithZ(z);
        var direction = rotation.ToDirection(random);
        return context => context.PlaceGameObject(blueprint, position, direction);
    }
}
