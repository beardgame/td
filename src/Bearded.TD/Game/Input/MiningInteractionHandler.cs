using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Input;

sealed class MiningInteractionHandler : InteractionHandler
{
    private readonly Faction faction;

    public MiningInteractionHandler(GameInstance game, Faction faction) : base(game)
    {
        this.faction = faction;
    }

    public override void Update(ICursorHandler cursor)
    {
        var currentTile = cursor.CurrentFootprint;
        if (!currentTile.IsValid(Game.State.Level)) return;

        if (cursor.Click.Hit)
        {
            foreach (var tile in currentTile.OccupiedTiles.Where(
                         t => Game.State.GeometryLayer[t].Type == TileType.Wall))
            {
                Game.Request(MineTile.Request(Game, faction, tile));
            }
        }
        else if (cursor.Cancel.Hit)
        {
            Game.PlayerInput.ResetInteractionHandler();
        }
    }
}
