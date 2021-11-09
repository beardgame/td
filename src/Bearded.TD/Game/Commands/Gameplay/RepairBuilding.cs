using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class RepairBuilding
    {
        public static IRequest<Player, GameInstance> Request(
            GameInstance game, Faction faction, ComponentGameObject building)
            => new Implementation(game, faction, building, Id<IWorkerTask>.Invalid);

        private sealed class Implementation : UnifiedRequestCommand
        {
            private readonly GameInstance game;
            private readonly Faction faction;
            private readonly ComponentGameObject building;
            private readonly Id<IWorkerTask> taskId;

            public Implementation(
                GameInstance game, Faction faction, ComponentGameObject building, Id<IWorkerTask> taskId)
            {
                this.game = game;
                this.faction = faction;
                this.building = building;
                this.taskId = taskId;
            }

            public override bool CheckPreconditions(Player actor)
            {
                return faction.SharesBehaviorWith<FactionResources>(actor.Faction) &&
                    faction.SharesBehaviorWith<WorkerTaskManager>(actor.Faction) &&
                    faction.SharesBehaviorWith<WorkerNetwork>(actor.Faction) &&
                    building.TryGetSingleComponent<IRuined>(out var ruined) &&
                    ruined.CanBeRepairedBy(faction);
            }

            public override ISerializableCommand<GameInstance> ToCommand() => new Implementation(
                game, faction, building, game.Meta.Ids.GetNext<IWorkerTask>());

            public override void Execute()
            {
                if (!building.TryGetSingleComponent<IRuined>(out var ruined))
                {
                    throw new NotSupportedException("Cannot repair a building without ruined component.");
                }

                if (!faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources))
                {
                    throw new NotSupportedException("Cannot build building without resources.");
                }

                if (!faction.TryGetBehaviorIncludingAncestors<WorkerTaskManager>(out var workers))
                {
                    throw new NotSupportedException("Cannot build building without workers.");
                }

                var incompleteRepair = ruined.StartRepair(faction);
                workers.RegisterTask(
                    new RepairWorkerTask(
                        taskId,
                        incompleteRepair,
                        OccupiedTileAccumulator.AccumulateOccupiedTiles(building),
                        game.State,
                        resources.ReserveResources(new FactionResources.ResourceRequest(incompleteRepair.Cost))));
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, building, taskId);
        }

        private sealed class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Faction> faction;
            private Id<ComponentGameObject> building;
            private Id<IWorkerTask> taskId;

            [UsedImplicitly]
            public Serializer() { }

            public Serializer(Faction faction, ComponentGameObject building, Id<IWorkerTask> taskId)
            {
                this.faction = faction.Id;
                this.building = building.FindId();
                this.taskId = taskId;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game) =>
                new Implementation(game, game.State.Factions.Resolve(faction), game.State.Find(building), taskId);

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
                stream.Serialize(ref building);
                stream.Serialize(ref taskId);
            }
        }
    }
}
