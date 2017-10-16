using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Mods.Models;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Commands
{
    static class SendUnitBlueprint
    {
        public static ICommand Command(GameInstance game, UnitBlueprint blueprint)
            => new Implementation(game, blueprint);

        private class Implementation : ICommand
        {
            private readonly GameInstance game;
            private readonly UnitBlueprint blueprint;

            public Implementation(GameInstance game, UnitBlueprint blueprint)
            {
                this.game = game;
                this.blueprint = blueprint;
            }

            public void Execute()
            {
                game.Blueprints.Units.RegisterBlueprint(blueprint);
            }

            public ICommandSerializer Serializer => new Serializer(blueprint);
        }

        private class Serializer : ICommandSerializer
        {
            private Id<UnitBlueprint> id;
            private string name;
            private int health;
            private int damage;
            private double timeBetweenAttacks;
            private float speed;
            private float value;
            private Color color;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(UnitBlueprint blueprint)
            {
                id = blueprint.Id;
                name = blueprint.Name;
                health = blueprint.Health;
                damage = blueprint.Damage;
                timeBetweenAttacks = blueprint.TimeBetweenAttacks.NumericValue;
                speed = blueprint.Speed.NumericValue;
                value = blueprint.Value;
                color = blueprint.Color;
            }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game, new UnitBlueprint(
                    id, name, health, damage, new TimeSpan(timeBetweenAttacks), new Speed(speed), value, color));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref id);
                stream.Serialize(ref name);
                stream.Serialize(ref health);
                stream.Serialize(ref damage);
                stream.Serialize(ref timeBetweenAttacks);
                stream.Serialize(ref speed);
                stream.Serialize(ref value);
                stream.Serialize(ref color);
            }
        }
    }
}
