using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class SendBuildingBlueprint
    {
        public static ICommand Command(GameInstance game, BuildingBlueprint blueprint)
            => new Implementation(game, blueprint);

        private class Implementation : ICommand
        {
            private readonly GameInstance game;
            private readonly BuildingBlueprint blueprint;

            public Implementation(GameInstance game, BuildingBlueprint blueprint)
            {
                this.game = game;
                this.blueprint = blueprint;
            }

            public void Execute()
            {
                game.Blueprints.Buildings.RegisterBlueprint(blueprint);
            }

            public ICommandSerializer Serializer => new Serializer(blueprint);
        }

        private class Serializer : ICommandSerializer
        {
            private Id<BuildingBlueprint> id;
            private string name;
            private Id<Footprint>[] footprints;
            private int maxHealth;
            private int resourceCost;
            private ICollection<Id<ComponentFactory>> components;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(BuildingBlueprint blueprint)
            {
                id = blueprint.Id;
                name = blueprint.Name;
                footprints = blueprint.Footprints.Footprints.Select(f => f.Id).ToArray();
                maxHealth = blueprint.MaxHealth;
                resourceCost = blueprint.ResourceCost;
                components = blueprint.ComponentFactories.Select(c => c.Id).ToList();
            }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game, getBuildingBlueprint(game));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref id);
                stream.Serialize(ref name);
                serializeFootprints(stream);
                stream.Serialize(ref maxHealth);
                stream.Serialize(ref resourceCost);
                stream.Serialize(ref components);
            }

            private void serializeFootprints(INetBufferStream stream)
            {
                var footprintCount = footprints.Length;
                stream.Serialize(ref footprintCount);
                if (footprintCount != footprints.Length)
                    footprints = new Id<Footprint>[footprintCount];
                for (var i = 0; i < footprintCount; i++)
                    stream.Serialize(ref footprints[i]);
            }

            private BuildingBlueprint getBuildingBlueprint(GameInstance game)
            {
                return new BuildingBlueprint(id, name, getFootprintGroup(game), maxHealth,
                    resourceCost, components.Select(cId => game.Blueprints.Components[cId]));
            }

            private FootprintGroup getFootprintGroup(GameInstance game)
            {
                return new FootprintGroup(footprints.Select(f => game.Blueprints.Footprints[f]));
            }
        }
    }
}
