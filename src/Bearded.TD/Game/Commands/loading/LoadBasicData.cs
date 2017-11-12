using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Buildings.Components;
using Bearded.TD.Game.Components.IPositionable;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;

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
                footprints.Add(FootprintGroup.Single);
                footprints.Add(FootprintGroup.CircleSeven);
                footprints.Add(FootprintGroup.Diamond);
                footprints.Add(FootprintGroup.Line);
                footprints.Add(FootprintGroup.Triangle);

                // Components
                var components = game.Blueprints.Components;
                components.Add(
                    new ComponentFactory("sink", () => new EnemySink()));
                components.Add(
                    new ComponentFactory("game_over_on_destroy", () => new GameOverOnDestroy()));
                components.Add(
                    new ComponentFactory("income_over_time", () => new IncomeOverTime()));
                components.Add(
                    new ComponentFactory("turret", () => new Turret(),
                        () => new TileVisibility(Turret.Range)));
                components.Add(
                    new ComponentFactory("worker_hub", () => new WorkerHub()));
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
