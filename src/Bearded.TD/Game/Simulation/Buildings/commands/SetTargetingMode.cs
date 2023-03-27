using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings;

static class SetTargetingMode
{
    public static IRequest<Player, GameInstance> Request(GameObject gameObject, ITargetingMode targetingMode) =>
        new Implementation(gameObject, targetingMode);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly GameObject gameObject;
        private readonly ITargetingMode targetingMode;

        public Implementation(GameObject gameObject, ITargetingMode targetingMode)
        {
            this.gameObject = gameObject;
            this.targetingMode = targetingMode;
        }

        public override bool CheckPreconditions(Player actor) =>
            gameObject.GetComponents<ITargetingModeSetter>().SingleOrDefault() is { } targeting &&
            targeting.AllowedTargetingModes.Contains(targetingMode);

        public override void Execute()
        {
            if (gameObject.GetComponents<ITargetingModeSetter>().SingleOrDefault() is { } targeting)
            {
                targeting.SetTargetingMode(targetingMode);
            }
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(gameObject, targetingMode);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<GameObject> gameObject;
        private int targetingModeIndex;

        // ReSharper disable once UnusedMember.Local
        public Serializer() { }

        public Serializer(GameObject gameObject, ITargetingMode targetingMode)
        {
            this.gameObject = gameObject.FindId();
            targetingModeIndex = TargetingMode.All.IndexOf(targetingMode);
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
        {
            return new Implementation(game.State.Find(gameObject), TargetingMode.All[targetingModeIndex]);
        }

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref gameObject);
            stream.Serialize(ref targetingModeIndex);
        }
    }
}
