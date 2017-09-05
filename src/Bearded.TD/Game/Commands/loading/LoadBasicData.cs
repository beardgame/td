using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Buildings.Components;
using Bearded.TD.Game.Components.IPositionable;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class LoadBasicData
    {
        public static ICommand Command(GameInstance game)
            => new Implementation(game);

        private class Implementation : ICommand
        {
            private readonly GameInstance game;

            public Implementation(GameInstance game)
            {
                this.game = game;
            }

            public void Execute()
            {
                // Footprints
                var footprints = game.Blueprints.Footprints;
                footprints.RegisterBlueprint(Footprint.Single);
                footprints.RegisterBlueprint(Footprint.TriangleUp);
                footprints.RegisterBlueprint(Footprint.TriangleDown);
                footprints.RegisterBlueprint(Footprint.CircleSeven);

                // Components
                var components = game.Blueprints.Components;
                components.RegisterBlueprint(
                    new ComponentFactory(new Id<ComponentFactory>(1), "sink", () => new EnemySink()));
                components.RegisterBlueprint(
                    new ComponentFactory(
                        new Id<ComponentFactory>(2), "game_over_on_destroy", () => new GameOverOnDestroy()));
                components.RegisterBlueprint(
                    new ComponentFactory(
                        new Id<ComponentFactory>(3), "income_over_time", () => new IncomeOverTime()));
                components.RegisterBlueprint(
                    new ComponentFactory(new Id<ComponentFactory>(4), "turret", () => new Turret(), () => new TileVisibility(Turret.Range)));
            }

            public ICommandSerializer Serializer => new Serializer();
        }

        private class Serializer : ICommandSerializer
        {
            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game);

            public void Serialize(INetBufferStream stream)
            {
            }
        }
    }
}
