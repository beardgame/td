using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Buildings;

static class GiveBuildingXp
{
    public static IRequest<Player, GameInstance> Request(GameInstance game, Experience amount)
        => new Implementation(game, amount);

    private sealed class Implementation : UnifiedDebugRequestCommand
    {
        private readonly GameInstance game;
        private readonly Experience amount;

        public Implementation(GameInstance game, Experience amount)
        {
            this.game = game;
            this.amount = amount;
        }

        public override void Execute()
        {
            foreach (var building in game.State.GameObjects)
            {
                if (building.TryGetSingleComponent<Veterancy.Veterancy>(out var veterancy))
                {
                    veterancy.AddXp(amount);
                }
            }
        }

        protected override bool CheckPreconditionsDebug(Player actor) => amount >= Experience.Zero;

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(amount);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private int experienceAmount;

        [UsedImplicitly]
        public Serializer() {}

        public Serializer(Experience amount)
        {
            experienceAmount = amount.NumericValue;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
        {
            return new Implementation(game, experienceAmount.Xp());
        }

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref experienceAmount);
        }
    }
}
