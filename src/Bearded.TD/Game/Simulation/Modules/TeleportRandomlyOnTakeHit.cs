using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

interface ITeleporter
{
    void Teleport(Position3 destination);
}

[Component("teleportRandomlyOnTakeHit")]
sealed class TeleportRandomlyOnTakeHit
    : Component<TeleportRandomlyOnTakeHit.IParameters>, IListener<TakeHit>, ITeleporter
{
    private Instant nextPossibleTeleportTime;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        Unit MaxDistance { get; }
        Unit MinDistance { get; }
        TimeSpan Cooldown { get; }
        IGameObjectBlueprint? SpawnObjectBefore { get; }
        IGameObjectBlueprint? SpawnObjectAfter { get; }
    }

    public TeleportRandomlyOnTakeHit(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(TakeHit @event)
    {
        tryTeleport();
    }

    private void tryTeleport()
    {
        if (nextPossibleTeleportTime > Owner.Game.Time)
            return;

        teleport();

        nextPossibleTeleportTime = Owner.Game.Time + Parameters.Cooldown;
    }

    private void teleport()
    {
        Owner.Sync(() =>
        {
            var passability = Owner.Game.PassabilityObserver.GetLayer(Passability.WalkingUnit);

            var maxTileRadius = MoreMath.CeilToInt(Parameters.MaxDistance.NumericValue + 1);

            var currentTile = Level.GetTile(Owner.Position);
            var possibleTiles = Tilemap
                .GetSpiralCenteredAt(currentTile, maxTileRadius)
                .Where(isBetweenMinAndMaxDistance)
                .Where(t => passability[t].IsPassable)
                .ToList();

            if (possibleTiles.Count == 0)
                return null;

            var destinationTile = possibleTiles.RandomElement();
            var destination = Level.GetPosition(destinationTile).WithZ();

            return TeleportGameObject.Command(Owner, destination);
        });
    }

    private bool isBetweenMinAndMaxDistance(Tile tile)
    {
        var p = Owner.Position.XY();
        var tp = Level.GetPosition(tile);
        var distanceSquared = (p - tp).LengthSquared;

        var error = 0.5.U();

        return distanceSquared > (Parameters.MinDistance - error).Squared
            && distanceSquared < (Parameters.MaxDistance + error).Squared;
    }

    void ITeleporter.Teleport(Position3 destination)
    {
        trySpawn(Parameters.SpawnObjectBefore, Owner.Position);

        Owner.Position = destination;

        trySpawn(Parameters.SpawnObjectAfter, destination);
    }

    private void trySpawn(IGameObjectBlueprint? blueprint, Position3 position)
    {
        if (blueprint == null)
            return;

        var obj = GameObjectFactory
            .CreateFromBlueprintWithDefaultRenderer(blueprint, Owner, position);
        Owner.Game.Add(obj);
    }
}

