﻿using Bearded.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.General
{
    static class AddFaction
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, Faction faction)
            => new Implementation(game, faction);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly Faction faction;

            public Implementation(GameInstance game, Faction faction)
            {
                this.game = game;
                this.faction = faction;
            }

            public void Execute()
            {
                game.State.AddFaction(faction);
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(faction);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<Faction> id;
            private Id<Faction> parent;
            private bool hasResources;
            private bool hasWorkerNetwork;
            private bool hasWorkers;
            private string name = "";
            private Color? color;

            public Serializer(Faction faction)
            {
                id = faction.Id;
                parent = faction.Parent?.Id ?? Id<Faction>.Invalid;
                hasResources = faction.HasResources;
                hasWorkerNetwork = faction.HasWorkerNetwork;
                hasWorkers = faction.HasWorkers;
                name = faction.Name;
                color = faction.Color;
            }

            [UsedImplicitly]
            public Serializer() {}

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(
                    game,
                    new Faction(
                        id,
                        game.State,
                        parent.IsValid ? game.State.FactionFor(parent) : null,
                        hasResources: hasResources,
                        hasWorkerNetwork: hasWorkerNetwork,
                        hasWorkers: hasWorkers,
                        name: name,
                        color: color));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref id);
                stream.Serialize(ref parent);
                stream.Serialize(ref hasResources);
                stream.Serialize(ref hasWorkerNetwork);
                stream.Serialize(ref hasWorkers);
                stream.Serialize(ref name);
                stream.Serialize(ref color);
            }
        }
    }
}
