﻿using Bearded.TD.Game.Buildings;
using Bearded.TD.Mods.Models;
using Bearded.TD.Tiles;

namespace Bearded.TD.Mods
{
    sealed class Mod
    {
        public string Name { get; }
        public string Id { get; }

        public ReadonlyBlueprintCollection<Footprint> Footprints { get; }
        public ReadonlyBlueprintCollection<ComponentFactory> Components { get; }
        public ReadonlyBlueprintCollection<BuildingBlueprint> Buildings { get; }
        public ReadonlyBlueprintCollection<UnitBlueprint> Units { get; }

        public Mod()
        {
            
        }

        public Mod(ReadonlyBlueprintCollection<UnitBlueprint> units)
        {
            Units = units;
        }
    }
}
