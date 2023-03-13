using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Elements.Phenomena;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;
using JetBrains.Annotations;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

static class StunBuilding
{
    public static IRequest<Player, GameInstance> Request(GameInstance game, TimeSpan duration) =>
        new Implementation(game, duration);

    private sealed class Implementation : UnifiedDebugRequestCommand
    {
        private readonly GameInstance game;
        private readonly TimeSpan duration;

        public Implementation(GameInstance game, TimeSpan duration)
        {
            this.game = game;
            this.duration = duration;
        }

        public override void Execute()
        {
            foreach (var building in game.State.GameObjects)
            {
                if (building.TryGetSingleComponent<IBuildingStateProvider>(out var stateProvider)
                    && stateProvider.State.IsCompleted
                    && building.TryGetSingleComponent<IElementSystemEntity>(out var entity))
                {
                    entity.ApplyEffect(new Stunned.Effect(duration));
                }
            }
        }

        protected override bool CheckPreconditionsDebug(Player actor) => duration > TimeSpan.Zero;

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(duration);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private double duration;

        [UsedImplicitly]
        public Serializer() {}

        public Serializer(TimeSpan duration)
        {
            this.duration = duration.NumericValue;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
        {
            return new Implementation(game, duration.S());
        }

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref duration);
        }
    }
}
