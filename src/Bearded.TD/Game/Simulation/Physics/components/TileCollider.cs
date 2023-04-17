using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Game.Simulation.World.TileType;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("tileCollider")]
sealed class TileCollider : Component<TileCollider.IParameters>, IPreviewListener<PreviewMove>
{
    private IRadius? radiusProvider;
    private Unit radius => radiusProvider?.Radius ?? Unit.Zero;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        bool WithWalls { get; }
        bool WithCrevices { get; }
        bool WithBuildings { get; }
    }

    public TileCollider(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
        ComponentDependencies.Depend<IRadius>(Owner, Events, r => radiusProvider = r);
    }

    public void PreviewEvent(ref PreviewMove move)
    {
        var collideWithGeometry = Parameters.WithWalls | Parameters.WithCrevices;
        var geometry = Owner.Game.GeometryLayer;
        var buildings = Owner.Game.BuildingLayer;

        var step = move.Step;
        step += step.NumericValue.NormalizedSafe() * radius;

        var ray = new Ray(move.Start.XY(), step.XY());

        var rayCaster = Owner.Game.Level.Cast(ray);

        while (rayCaster.MoveNext(out var tile))
        {
            if (collideWithGeometry)
            {
                var tileType = geometry[tile].Geometry.Type;
                var collidedWithWall = Parameters.WithWalls && tileType == Wall;
                var collidedWithCrevice = Parameters.WithCrevices && tileType == Crevice;

                if (collidedWithWall || collidedWithCrevice)
                {
                    collide(ref move, step, rayCaster);
                    return;
                }
            }

            if (Parameters.WithBuildings && buildings.HasMaterializedBuilding(tile))
            {
                collide(ref move, step, rayCaster);
                return;
            }
        }
    }

    private void collide(ref PreviewMove move, Difference3 step, in LevelRayCaster rayCaster)
    {
        HitLevel(Events, move.Start, move.Step, rayCaster.LastStep, rayCaster.Current);
        updateMoveOnCollision(ref move, step, rayCaster);
    }

    private void updateMoveOnCollision(ref PreviewMove move, Difference3 step, in LevelRayCaster rayCaster)
    {
        var radiusOffset = getRadiusVectorInDirection(step);

        move = move with { Step = step * rayCaster.CurrentRayFactor - radiusOffset };
    }

    private Difference3 getRadiusVectorInDirection(Difference3 direction)
    {
        return direction.NumericValue.NormalizedSafe() * radius;
    }

    public static void HitLevel(ComponentEvents events, Position3 point, Difference3 step, Direction? withStep, Tile tile)
    {
        var normal = new Difference3(withStep?.Vector().WithZ() ?? Vector3.UnitZ);
        var info = new Impact(point, normal, step.NormalizedSafe());
        events.Send(new CollideWithLevel(info, tile));
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}

