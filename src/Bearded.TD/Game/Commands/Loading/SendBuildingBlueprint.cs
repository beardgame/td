using System;
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
            private Id<FootprintGroup> footprint;
            private int maxHealth;
            private int resourceCost;
            // missing: components

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(BuildingBlueprint blueprint)
            {
                id = blueprint.Id;
                name = blueprint.Name;
                footprint = blueprint.Footprints.Id;
                maxHealth = blueprint.MaxHealth;
                resourceCost = blueprint.ResourceCost;
            }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game, getBuildingBlueprint(game));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref id);
                stream.Serialize(ref name);
                stream.Serialize(ref footprint);
                stream.Serialize(ref maxHealth);
                stream.Serialize(ref resourceCost);
                // missing: components
            }

            private BuildingBlueprint getBuildingBlueprint(GameInstance game)
            {
                return new BuildingBlueprint(id, name, FootprintGroup.FootprintsById[footprint], maxHealth,
                    resourceCost, new Func<Component>[0]);
            }
        }
    }
}
