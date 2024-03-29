using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Testing.Components;
using Bearded.TD.Testing.Factions;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Game.Simulation.Buildings.Ruins;

public sealed class RuinedTest
{
    private readonly ComponentTestBed testBed;
    private readonly IBuildingState buildingState;

    public RuinedTest()
    {
        testBed = ComponentTestBed.CreateInGame();
        var stateManager = new BuildingStateManager();
        testBed.AddComponent(stateManager);
        testBed.SendEvent(new ConstructionFinished());
        buildingState = stateManager.State;
    }

    [Fact]
    public void BuildingWithoutRuinedComponentIsFunctional()
    {
        buildingState.IsFunctional.Should().BeTrue();
    }

    [Fact]
    public void AddingRuinedComponentMakesBuildingNonFunctional()
    {
        testBed.AddComponent(new Ruined());

        buildingState.IsFunctional.Should().BeFalse();
    }

    [Fact]
    public void RemovingRuinedComponentMakesBuildingFunctional()
    {
        var ruined = new Ruined();
        testBed.AddComponent(ruined);

        testBed.RemoveComponent(ruined);

        buildingState.IsFunctional.Should().BeTrue();
    }

    [Fact]
    public void RepairFinishedEventRestoresBuildingToFunctional()
    {
        var faction = FactionTestFactory.CreateFaction();
        testBed.AddComponent(new Ruined());

        testBed.SendEvent(new RepairFinished(faction));
        testBed.AdvanceSingleFrame();

        buildingState.IsFunctional.Should().BeTrue();
    }

    [Fact]
    public void RepairFinishedEventConvertsFactionToRepairingFaction()
    {
        var originalFaction = FactionTestFactory.CreateFaction();
        var factionProvider = new FactionProvider(originalFaction);
        testBed.AddComponent(factionProvider);
        testBed.AddComponent(new Ruined());
        var repairingFaction = FactionTestFactory.CreateFaction();

        testBed.SendEvent(new RepairFinished(repairingFaction));

        factionProvider.Faction.Should().Be(repairingFaction);
    }
}
