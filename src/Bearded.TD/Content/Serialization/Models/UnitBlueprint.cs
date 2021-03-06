﻿using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Units;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    sealed class UnitBlueprint : IConvertsTo<Content.Models.UnitBlueprint, Void>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Health { get; set; }
        public int Damage { get; set; }
        public TimeSpan TimeBetweenAttacks { get; set; }
        public Speed Speed { get; set; }
        public float Value { get; set; }
        public Color Color { get; set; }
        public List<IComponent> Components { get; set; }

        public Content.Models.UnitBlueprint ToGameModel(ModMetadata modMetadata, Void _)
        {
            return new(
                ModAwareId.FromNameInMod(Id, modMetadata),
                Name,
                Health,
                Damage,
                TimeBetweenAttacks,
                Speed,
                Value,
                Color,
                Components?.Select(ComponentFactories.CreateComponentFactory<EnemyUnit>));
        }
    }
}
