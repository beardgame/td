using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Testing.Components;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Game.Simulation.Units;

public sealed class NotifyWhenStuckTests
{
    private readonly ComponentTestBed componentTestBed;
    private readonly MockEnemyMovement enemyMovement;

    public NotifyWhenStuckTests()
    {
        componentTestBed = ComponentTestBed.CreateInGame();
        enemyMovement = new MockEnemyMovement();
        componentTestBed.AddComponent(enemyMovement);
    }

    [Fact]
    public void StuckEventIsSentAfterDelay()
    {
        componentTestBed.AddComponent(notifyWhenStuckComponent(0.5.S()));
        var events = componentTestBed.CollectEvents<EnemyGotStuck>();

        enemyMovement.IsMoving = false;
        componentTestBed.AdvanceFramesFor(1.S());

        events.Should().HaveCount(1);
    }

    [Fact]
    public void StuckEventIsNotSentBeforeDelay()
    {
        componentTestBed.AddComponent(notifyWhenStuckComponent(0.5.S()));
        var events = componentTestBed.CollectEvents<EnemyGotStuck>();

        enemyMovement.IsMoving = false;
        componentTestBed.AdvanceFramesFor(0.25.S());

        events.Should().BeEmpty();
    }

    [Fact]
    public void StuckEventIsNotSentWhenStartedMovingBeforeDelay()
    {
        componentTestBed.AddComponent(notifyWhenStuckComponent(0.5.S()));
        var events = componentTestBed.CollectEvents<EnemyGotStuck>();

        enemyMovement.IsMoving = false;
        componentTestBed.AdvanceFramesFor(0.25.S());

        enemyMovement.IsMoving = true;
        componentTestBed.AdvanceFramesFor(1.S());

        events.Should().BeEmpty();
    }

    [Fact]
    public void StuckEventIsSentImmediatelyIfNoDelay()
    {
        componentTestBed.AddComponent(notifyWhenStuckComponent(TimeSpan.Zero));
        var events = componentTestBed.CollectEvents<EnemyGotStuck>();

        enemyMovement.IsMoving = false;
        componentTestBed.AdvanceSingleFrame();

        events.Should().HaveCount(1);
    }

    [Fact]
    public void UnstuckEventIsSentImmediately()
    {
        componentTestBed.AddComponent(notifyWhenStuckComponent(TimeSpan.Zero));
        var events = componentTestBed.CollectEvents<EnemyGotUnstuck>();

        enemyMovement.IsMoving = false;
        componentTestBed.AdvanceSingleFrame();

        enemyMovement.IsMoving = true;
        componentTestBed.AdvanceSingleFrame();

        events.Should().HaveCount(1);
    }

    [Fact]
    public void UnstuckEventIsNotSentIfNotStuckBeforeDelay()
    {
        componentTestBed.AddComponent(notifyWhenStuckComponent(0.5.S()));
        var events = componentTestBed.CollectEvents<EnemyGotUnstuck>();

        enemyMovement.IsMoving = false;
        componentTestBed.AdvanceFramesFor(0.25.S());

        enemyMovement.IsMoving = true;
        componentTestBed.AdvanceSingleFrame();

        events.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1, 8, Direction.Right)]
    [InlineData(-3, -2, Direction.UpLeft)]
    [InlineData(109, -26, Direction.DownLeft)]
    public void StuckEventContainsNeighborTile(int tileX, int tileY, Direction dir)
    {
        var tile = new Tile(tileX, tileY);

        componentTestBed.AddComponent(notifyWhenStuckComponent(TimeSpan.Zero));
        var events = componentTestBed.CollectEvents<EnemyGotStuck>();

        componentTestBed.MoveObject(Level.GetPosition(tile).WithZ());
        enemyMovement.TileDirection = dir;
        enemyMovement.IsMoving = false;
        componentTestBed.AdvanceSingleFrame();

        events.Should().Equal(new EnemyGotStuck(tile.Neighbor(dir)));
    }

    private static NotifyWhenStuck notifyWhenStuckComponent(TimeSpan delay) =>
        new(new NotifyWhenStuckParametersTemplate(delay));

    private sealed class MockEnemyMovement : Component, IEnemyMovement
    {
        public bool IsMoving { get; set; } = true;
        public Direction TileDirection { get; set; }

        protected override void OnAdded() {}
        public override void Update(TimeSpan elapsedTime) {}
    }
}
