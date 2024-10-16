﻿using Bearded.TD.Audio;
using Bearded.TD.Content;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.UI.Controls;

namespace Bearded.TD.Game.Input;

sealed class BuildingInteractionHandler : InteractionHandler
{
    private readonly Faction faction;
    private readonly IGameObjectBlueprint blueprint;
    private readonly ContentManager contentManager;
    private readonly GridVisibility gridVisibility;
    protected override TileSelection TileSelection { get; }
    private GameObject? ghost;
    private IActiveOverlay? towerRangeOverlay;
    private DynamicFootprintTileNotifier? ghostTileOccupation;

    public BuildingInteractionHandler(
        GameInstance game,
        Faction faction,
        IGameObjectBlueprint blueprint,
        ContentManager contentManager,
        GridVisibility gridVisibility) : base(game)
    {
        this.faction = faction;
        this.blueprint = blueprint;
        this.contentManager = contentManager;
        this.gridVisibility = gridVisibility;
        TileSelection = TileSelection.FromFootprint(blueprint.GetFootprint());
    }

    protected override void OnStart(ICursorHandler cursor)
    {
        ghost = BuildingFactory.CreateGhost(blueprint, faction, out ghostTileOccupation);
        Game.State.Add(ghost);
        Game.PlayerCursors.AttachGhost(blueprint);

        gridVisibility.OnStartBuilding(ghost);
        towerRangeOverlay =
            TowerRangeOverlayLayer.CreateAndActivateForGameObject(Game.Overlays, ghost, RangeDrawStyle.DrawFull);
    }

    public override void Update(ICursorHandler cursor)
    {
        var footprint = cursor.CurrentFootprint;
        ghostTileOccupation?.SetFootprint(footprint);
        if (cursor.Click.Hit)
        {
            if (cursor.ModifierKeys.IsSupersetOf(Constants.Input.DebugForceModifier))
            {
                Game.Request(ForceBuildBuilding.Request, faction, blueprint, footprint);
            }
            else
            {
                Game.Request(BuildBuilding.Request, faction, blueprint, footprint);

                var sound = contentManager.ResolveSoundEffect(Constants.Content.CoreUI.Sounds.UpgradeGeneric);
                Game.Meta.SoundScape.PlayGlobalSound(sound);
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

        gridVisibility.OnEndBuilding();

        towerRangeOverlay?.Deactivate();
        towerRangeOverlay = null;
    }
}
