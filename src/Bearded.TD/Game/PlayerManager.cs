﻿using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking;
using Bearded.TD.Networking.Serialization;
using Lidgren.Network;

namespace Bearded.TD.Game
{
    sealed class PlayerManager : INetworkMessageHandler
    {
        private const double timeBetweenSyncsInS = .5;

        private readonly GameInstance game;
        private readonly ServerNetworkInterface networkInterface;
        private readonly IDispatcher<GameInstance> dispatcher;

        private readonly Dictionary<NetConnection, Player> connectionToPlayer = new Dictionary<NetConnection, Player>();
        private readonly Dictionary<Player, NetConnection> playerToConnection = new Dictionary<Player, NetConnection>();

        private double timeUntilNextSync;

        public PlayerManager(
                GameInstance game, ServerNetworkInterface networkInterface, IDispatcher<GameInstance> dispatcher)
        {
            this.game = game;
            this.networkInterface = networkInterface;
            this.dispatcher = dispatcher;

            game.Me.LastKnownPing = 0;
            timeUntilNextSync = timeBetweenSyncsInS;
        }

        public void Update(UpdateEventArgs args)
        {
            timeUntilNextSync -= args.ElapsedTimeInS;
            if (timeUntilNextSync <= 0)
            {
                syncPlayers();
                timeUntilNextSync = timeBetweenSyncsInS;
            }
        }

        private void syncPlayers()
        {
            // Update ping for all players.
            foreach (var p in game.Players)
            {
                if (p == game.Me)
                {
                    p.LastKnownPing = 0;
                    continue;
                }
                p.LastKnownPing = (int) (playerToConnection[p].AverageRoundtripTime * 1000);
            }

            dispatcher.RunOnlyOnServer(SyncPlayers.Command, game);
        }

        private Player getSender(NetIncomingMessage msg) => connectionToPlayer[msg.SenderConnection];

        private void addPlayerConnection(Player player, NetConnection connection)
        {
            connectionToPlayer.Add(connection, player);
            playerToConnection.Add(player, connection);
        }

        private void removePlayerConnection(NetConnection conn)
        {
            var player = connectionToPlayer[conn];
            connectionToPlayer.Remove(conn);
            playerToConnection.Remove(player);
        }

        public bool Accepts(NetIncomingMessage message)
        {
            var msgType = message.MessageType;
            return msgType == NetIncomingMessageType.ConnectionApproval ||
                    msgType == NetIncomingMessageType.StatusChanged;
        }

        public void Handle(NetIncomingMessage message)
        {
            switch (message.MessageType)
            {
                case NetIncomingMessageType.ConnectionApproval:
                    handleConnectionApproval(message);
                    break;
                case NetIncomingMessageType.StatusChanged:
                    handleStatusChange(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void handleConnectionApproval(NetIncomingMessage msg)
        {
            var clientInfo = ClientInfo.FromBuffer(msg.SenderConnection.RemoteHailMessage);
            if (!checkClientInfo(clientInfo, out var rejectionReason))
            {
                msg.SenderConnection.Deny(rejectionReason);
                return;
            }
            var newPlayer = new Player(game.Ids.GetNext<Player>(), clientInfo.PlayerName);
            dispatcher.RunOnlyOnServer(
                commandDispatcher => commandDispatcher.Dispatch(AddPlayer.Command(game, newPlayer)));
            newPlayer.ConnectionState = PlayerConnectionState.Connecting;
            addPlayerConnection(newPlayer, msg.SenderConnection);
            sendApproval(newPlayer, msg.SenderConnection);
        }

        private bool checkClientInfo(ClientInfo clientInfo, out string rejectionReason)
        {
            if (clientInfo.PlayerName.Trim() == "")
            {
                rejectionReason = "Empty name is not allowed";
                return false;
            }

            rejectionReason = null;
            return true;
        }

        private void sendApproval(Player player, NetConnection connection)
        {
            var msg = networkInterface.CreateMessage();
            var info = LobbyPlayerInfo.ForPlayer(player);
            info.Serialize(new NetBufferWriter(msg));
            connection.Approve(msg);
        }

        private void handleStatusChange(NetIncomingMessage msg)
        {
            var status = (NetConnectionStatus) msg.ReadByte();
            switch (status)
            {
                case NetConnectionStatus.Connected:
                    getSender(msg).ConnectionState = PlayerConnectionState.Waiting;
                    
                    foreach (var p in game.Players)
                    {
                        if (p == getSender(msg)) continue;
                        sendCommandToConnection(msg.SenderConnection, AddPlayer.Command(game, p));
                    }

                    sendCommandToConnection(msg.SenderConnection, SetGameSettings.Command(game, game.GameSettings));
                    
                    break;
                case NetConnectionStatus.Disconnected:
                    game.RemovePlayer(getSender(msg));
                    removePlayerConnection(msg.SenderConnection);
                    break;
            }
        }

        private void sendCommandToConnection(NetConnection conn, ISerializableCommand<GameInstance> command)
        {
            var outMsg = networkInterface.CreateMessage();
            CommandToNetworkMessageWriter.WriteCommandToMessage(command, outMsg);
            networkInterface.SendMessageToConnection(conn, outMsg, NetworkChannel.Chat);
        }
    }
}
