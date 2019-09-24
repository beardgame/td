﻿using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Mods;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    sealed class BuildingBlueprint
        : IConvertsTo<Content.Models.BuildingBlueprint, UpgradeTagResolver>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Game.World.FootprintGroup Footprint { get; set; }
        public int Cost { get; set; }
        public List<string> Tags { get; set; }
        public List<IBuildingComponent> Components { get; set; }

        public Content.Models.BuildingBlueprint ToGameModel(UpgradeTagResolver tags)
        {
            return new Content.Models.BuildingBlueprint(
                Id,
                Name,
                Footprint,
                Cost,
                Tags?.Select(tags.Resolve),
                Components?.Select(ComponentFactories.CreateBuildingComponentFactory)
            );
        }
    }
}
