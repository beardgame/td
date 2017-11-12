using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Mods.Models;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;

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
                game.Blueprints.Buildings.Add(blueprint);
            }

            public ICommandSerializer Serializer => new Serializer(blueprint);
        }

        private class Serializer : ICommandSerializer
        {
            private string name;
            private string[] footprints = new string[0];
            private int maxHealth;
            private int resourceCost;
            private string[] components = new string[0];

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(BuildingBlueprint blueprint)
            {
                name = blueprint.Name;
                footprints = blueprint.Footprints.Footprints.Select(f => f.Name).ToArray();
                maxHealth = blueprint.MaxHealth;
                resourceCost = blueprint.ResourceCost;
                components = blueprint.ComponentFactories.Select(c => c.Name).ToArray();
            }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game, getBuildingBlueprint(game));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref name);
                serializeFootprints(stream);
                stream.Serialize(ref maxHealth);
                stream.Serialize(ref resourceCost);
                serializeComponents(stream);
            }

            private void serializeFootprints(INetBufferStream stream)
            {
                var footprintCount = footprints.Length;
                stream.Serialize(ref footprintCount);
                if (footprintCount != footprints.Length)
                    footprints = new string[footprintCount];
                for (var i = 0; i < footprintCount; i++)
                    stream.Serialize(ref footprints[i]);
            }

            private void serializeComponents(INetBufferStream stream)
            {
                var componentCount = components.Length;
                stream.Serialize(ref componentCount);
                if (componentCount != components.Length)
                    components = new string[componentCount];
                for (var i = 0; i < componentCount; i++)
                    stream.Serialize(ref components[i]);
            }

            private BuildingBlueprint getBuildingBlueprint(GameInstance game)
            {
                return new BuildingBlueprint(name, getFootprintGroup(game), maxHealth,
                    resourceCost, components.Select(cId => game.Blueprints.Components[cId]));
            }

            private FootprintGroup getFootprintGroup(GameInstance game)
            {
                return new FootprintGroup(footprints.Select(f => game.Blueprints.Footprints[f]));
            }
        }
    }
}
