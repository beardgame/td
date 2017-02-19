﻿using System.Linq;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.UI
{
    class DebugSpawnEnemyClickHandler : IClickHandler
    {
        public TileSelection Selection => TileSelection.Single;

        public void HandleHover(GameState game, PositionedFootprint footprint)
        { }

        public void HandleClick(GameState game, PositionedFootprint footprint)
        {
            footprint.OccupiedTiles
                .Where(t => t.IsValid && t.Info.IsPassable)
                .ForEach(tile => game.Add(new EnemyUnit(new UnitBlueprint(100, 25, new Speed(2)), tile)));
        }

        public void Enable(GameState game)
        { }

        public void Disable(GameState game)
        { }
    }
}
