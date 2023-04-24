﻿using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Input;

sealed class BuildingInteractionHandler : InteractionHandler
{
    private readonly Faction faction;
    private readonly IGameObjectBlueprint blueprint;
    protected override TileSelection TileSelection { get; }
    private GameObject? ghost;
    private DynamicFootprintTileNotifier? ghostTileOccupation;

    public BuildingInteractionHandler(GameInstance game, Faction faction, IGameObjectBlueprint blueprint) : base(game)
    {
        this.faction = faction;
        this.blueprint = blueprint;
        TileSelection = TileSelection.FromFootprint(blueprint.GetFootprint());
    }

    protected override void OnStart(ICursorHandler cursor)
    {
        ghost = BuildingFactory.CreateGhost(blueprint, faction, out ghostTileOccupation);
        Game.State.Add(ghost);
        Game.PlayerCursors.AttachGhost(blueprint);
    }

    public override void Update(ICursorHandler cursor)
    {
        var footprint = cursor.CurrentFootprint;
        ghostTileOccupation?.SetFootprint(footprint);
        if (cursor.Click.Hit)
        {
            if (Constants.Input.DebugForceModifier(cursor.ModifierKeys))
            {
                Game.Request(ForceBuildBuilding.Request, faction, blueprint, footprint);
            }
            else
            {
                Game.Request(BuildBuilding.Request, faction, blueprint, footprint);
            }
        }
        else if (cursor.Cancel.Hit)
        {
            Game.PlayerInput.ResetInteractionHandler();
        }
    }

    protected override void OnEnd(ICursorHandler cursor)
    {
        ghost?.Delete();
        ghost = null;
        ghostTileOccupation = null;
        Game.PlayerCursors.DetachGhost();
    }
}
