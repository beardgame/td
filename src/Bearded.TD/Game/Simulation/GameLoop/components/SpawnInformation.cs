using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed class SpawnInformation : Component
{
    private readonly Tile tile;
    private Instant nextIndicatorSpawn;

    public SpawnInformation(Tile tile)
    {
        this.tile = tile;
    }

    protected override void OnAdded() { }

    public override void Activate()
    {
        Owner.Position = Level.GetPosition(tile).WithZ(Unit.Zero);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Owner.Game.Time >= nextIndicatorSpawn)
        {
            Owner.Game.Add(GameLoopObjectFactory.CreateEnemyPathIndicator(tile));
            nextIndicatorSpawn = Owner.Game.Time + Constants.Game.Enemy.TimeBetweenIndicators;
        }
    }
}
