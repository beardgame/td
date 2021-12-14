using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay;

static class MineTile
{
    public static IRequest<Player, GameInstance> Request(GameInstance game, Faction faction, Tile tile)
        => new Implementation(game, faction, tile, Id<IWorkerTask>.Invalid);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly GameInstance game;
        private readonly Faction faction;
        private readonly Tile tile;
        private readonly Id<IWorkerTask> taskId;

        public Implementation(GameInstance game, Faction faction, Tile tile, Id<IWorkerTask> taskId)
        {
            this.game = game;
            this.faction = faction;
            this.tile = tile;
            this.taskId = taskId;
        }

        public override bool CheckPreconditions(Player actor)
        {
            if (!faction.TryGetBehavior<WorkerTaskManager>(out var factionTaskManager))
            {
                return false;
            }
            actor.Faction.TryGetBehaviorIncludingAncestors<WorkerTaskManager>(out var actorTaskManager);
            return factionTaskManager == actorTaskManager && game.State.MiningLayer.CanTileBeMined(tile);
        }

        public override ISerializableCommand<GameInstance> ToCommand() =>
            new Implementation(game, faction, tile, game.Meta.Ids.GetNext<IWorkerTask>());

        public override void Execute()
        {
            var placeholder = new MiningTaskPlaceholder(faction, tile, taskId);
            game.State.Add(placeholder);
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(tile, faction, taskId);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private int tileX;
        private int tileY;
        private Id<Faction> faction;
        private Id<IWorkerTask> taskId;

        [UsedImplicitly]
        public Serializer()
        {
        }

        public Serializer(Tile tile, Faction faction, Id<IWorkerTask> taskId)
        {
            this.taskId = taskId;
            tileX = tile.X;
            tileY = tile.Y;
            this.faction = faction.Id;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
        {
            return new Implementation(game, game.State.Factions.Resolve(faction), new Tile(tileX, tileY), taskId);
        }

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref tileX);
            stream.Serialize(ref tileY);
            stream.Serialize(ref faction);
            stream.Serialize(ref taskId);
        }
    }
}